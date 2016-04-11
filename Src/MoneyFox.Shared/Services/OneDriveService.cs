﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OneDrive.Sdk;
using MoneyFox.Shared.Constants;
using MoneyFox.Shared.Extensions;
using MoneyFox.Shared.Interfaces;
using MvvmCross.Plugins.File;

namespace MoneyFox.Shared.Services
{
    public class OneDriveService : IBackupService
    {
        private readonly IMvxFileStore fileStore;
        private readonly IOneDriveAuthenticator oneDriveAuthenticator;

        public OneDriveService(IMvxFileStore fileStore, IOneDriveAuthenticator oneDriveAuthenticator)
        {
            this.fileStore = fileStore;
            this.oneDriveAuthenticator = oneDriveAuthenticator;
        }

        private IOneDriveClient OneDriveClient { get; set; }

        private Item BackupFolder { get; set; }

        public bool IsLoggedIn => OneDriveClient?.IsAuthenticated ?? false;

        public async Task Login()
        {
            if (!IsLoggedIn)
            {
                OneDriveClient = await oneDriveAuthenticator.LoginAsync();
            }

            if (OneDriveClient.IsAuthenticated)
            {
                await GetBackupFolder();
            }
        }

        public async Task<TaskCompletionType> Upload()
        {
            if (!IsLoggedIn)
            {
                await Login();
            }

            try
            {
                using (var dbstream = fileStore.OpenRead(OneDriveAuthenticationConstants.DB_NAME))
                {
                    var uploadedItem = await OneDriveClient
                        .Drive
                        .Root
                        .ItemWithPath(Path.Combine(OneDriveAuthenticationConstants.BACKUP_FOLDER_NAME,
                            OneDriveAuthenticationConstants.BACKUP_NAME))
                        .Content
                        .Request()
                        .PutAsync<Item>(dbstream);

                    return uploadedItem != null ? TaskCompletionType.Successful : TaskCompletionType.Unsuccessful;
                }
            }
            catch (OneDriveException)
            {
                return TaskCompletionType.Unsuccessful;
            }
        }

        public async Task<TaskCompletionType> Restore()
        {
            if (!IsLoggedIn)
            {
                await Login();
            }

            try
            {
                var children = await OneDriveClient.Drive.Items[BackupFolder?.Id].Children.Request().GetAsync();
                var existingBackup = children.FirstOrDefault(x => x.Name == OneDriveAuthenticationConstants.BACKUP_NAME);

                if (existingBackup != null)
                {
                    var backup = await OneDriveClient.Drive.Items[existingBackup.Id].Content.Request().GetAsync();
                    if (fileStore.Exists(OneDriveAuthenticationConstants.DB_NAME))
                    {
                        fileStore.DeleteFile(OneDriveAuthenticationConstants.DB_NAME);
                    }
                    fileStore.WriteFile(OneDriveAuthenticationConstants.DB_NAME, backup.ReadToEnd());
                }
            }
            catch (OneDriveException ex)
            {
                return TaskCompletionType.Unsuccessful;
            }

            return TaskCompletionType.Successful;
        }

        public async Task<DateTime> GetBackupDate()
        {
            if (!IsLoggedIn)
            {
                await Login();
            }

            var children = await OneDriveClient.Drive.Items[BackupFolder?.Id].Children.Request().GetAsync();
            var existingBackup = children.FirstOrDefault(x => x.Name == OneDriveAuthenticationConstants.BACKUP_NAME);

            if (existingBackup != null)
            {
                return existingBackup.LastModifiedDateTime?.DateTime ?? DateTime.MinValue;
            }

            return DateTime.MinValue;
        }

        private async Task GetBackupFolder()
        {
            var children = await OneDriveClient.Drive.Root.Children.Request().GetAsync();
            BackupFolder =
                children.CurrentPage.FirstOrDefault(x => x.Name == OneDriveAuthenticationConstants.BACKUP_FOLDER_NAME);

            if (BackupFolder == null)
            {
                await CreateBackupFolder();
            }
        }

        private async Task CreateBackupFolder()
        {
            var folderToCreate = new Item
            {
                Name = OneDriveAuthenticationConstants.BACKUP_FOLDER_NAME,
                Folder = new Folder()
            };

            var root = await OneDriveClient.Drive.Root.Request().GetAsync();

            BackupFolder = await OneDriveClient.Drive.Items[root.Id].Children.Request()
                .AddAsync(folderToCreate);
        }
    }
}