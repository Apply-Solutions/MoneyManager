﻿using MoneyFox.Foundation.Interfaces;
using MoneyFox.Foundation.Resources;
using MvvmCross.Core.ViewModels;
using MvvmCross.Localization;

namespace MoneyFox.Business.ViewModels
{
    public class SettingsSecurityViewModel : BaseViewModel
    {
        private readonly ISettingsManager settingsManager;
        private readonly IDialogService dialogService;
        private readonly IPasswordStorage passwordStorage;
        private string password;
        private string passwordConfirmation;

        public SettingsSecurityViewModel(ISettingsManager settingsManager, IPasswordStorage passwordStorage, IDialogService dialogService)
        {
            this.settingsManager = settingsManager;
            this.passwordStorage = passwordStorage;
            this.dialogService = dialogService;
        }

        /// <summary>
        ///     Grants the GUI access to the password setting.
        /// </summary>
        public bool IsPasswortActive
        {
            get { return settingsManager.PasswordRequired; }
            set
            {
                settingsManager.PasswordRequired = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Provides an TextSource for the translation binding on this page.
        /// </summary>
        public IMvxLanguageBinder TextSource => new MvxLanguageBinder("", GetType().Name);

        /// <summary>
        ///     The password who the user set.
        /// </summary>
        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     The password confirmation the user entered.
        /// </summary>
        public string PasswordConfirmation
        {
            get { return passwordConfirmation; }
            set
            {
                passwordConfirmation = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Save the password to the secure storage of the current platform
        /// </summary>
        public MvxCommand SavePasswordCommand => new MvxCommand(SavePassword);

        /// <summary>
        ///     Loads the password from the secure storage
        /// </summary>
        public MvxCommand LoadCommand => new MvxCommand(LoadData);

        /// <summary>
        ///     Remove the password from the secure storage
        /// </summary>
        public MvxCommand UnloadCommand => new MvxCommand(RemovePassword);

        private void SavePassword()
        {
            if (Password != PasswordConfirmation)
            {
                dialogService.ShowMessage(Strings.PasswordConfirmationWrongTitle,
                    Strings.PasswordConfirmationWrongMessage);
                return;
            } else if (Password == "" || PasswordConfirmation == "")
            {
                dialogService.ShowMessage("Password Error",
                    "Please enter something into the password field.");
                return;
            }

            passwordStorage.SavePassword(Password);

            dialogService.ShowMessage(Strings.PasswordSavedTitle, Strings.PasswordSavedMessage);
        }

        private void LoadData()
        {
            if (IsPasswortActive)
            {
                Password = passwordStorage.LoadPassword();
                PasswordConfirmation = passwordStorage.LoadPassword();
            }
        }

        private void RemovePassword()
        {
            if (!IsPasswortActive)
            {
                passwordStorage.RemovePassword();
            }

            //  Deactivate option again if no password was entered
            if (IsPasswortActive && string.IsNullOrEmpty(Password))
            {
                IsPasswortActive = false;
            }
        }
    }
}