﻿using System.Diagnostics.CodeAnalysis;
using MoneyFox.ServiceLayer.Facades;
using MoneyFox.ServiceLayer.Interfaces;
using MoneyFox.ServiceLayer.ViewModels;
using Moq;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using Xunit;

namespace MoneyFox.ServiceLayer.Tests.ViewModels
{
    [ExcludeFromCodeCoverage]
    [Collection("MvxIocCollection")]
    public class SettingsBackgroundJobViewModelTests
    {
        [Theory]
        [InlineData(5, 5)]
        [InlineData(1, 1)]
        [InlineData(-2, 1)]
        [InlineData(24, 24)]
        [InlineData(48, 48)]
        public void BackupSyncRecurrence(int passedValue, int expectedValue)
        {
            // Arrange
            var settingsFacadeMock = new Mock<ISettingsFacade>();
            settingsFacadeMock.SetupAllProperties();

            var taskStarted = false;
            var backgroundTaskManager = new Mock<IBackgroundTaskManager>();
            backgroundTaskManager.Setup(x => x.StartBackupSyncTask(It.IsAny<int>())).Callback(() => taskStarted = true);

            // Act
            var vm = new SettingsBackgroundJobViewModel(settingsFacadeMock.Object, backgroundTaskManager.Object,
                new Mock<IMvxLogProvider>().Object,
                new Mock<IMvxNavigationService>().Object) {BackupSyncRecurrence = passedValue};

            // Assert
            Assert.True(taskStarted);
            Assert.Equal(expectedValue, vm.BackupSyncRecurrence);
        }

        [Fact]
        public void IsAutoBackupEnabled_StartService()
        {
            // Arrange
            var settingsFacadeMock = new Mock<ISettingsFacade>();
            settingsFacadeMock.SetupGet(x => x.IsBackupAutouploadEnabled).Returns(false);

            var taskStarted = false;
            var backgroundTaskManager = new Mock<IBackgroundTaskManager>();
            backgroundTaskManager.Setup(x => x.StartBackupSyncTask(It.IsAny<int>())).Callback(() => taskStarted = true);

            // Act
            var vm = new SettingsBackgroundJobViewModel(settingsFacadeMock.Object, backgroundTaskManager.Object,
                                                        new Mock<IMvxLogProvider>().Object,
                                                        new Mock<IMvxNavigationService>().Object)
            {
                IsAutoBackupEnabled = true
            };

            // Assert
            Assert.True(taskStarted);
        }

        [Fact]
        public void IsAutoBackupEnabled_StopService()
        {
            // Arrange
            var settingsFacadeMock = new Mock<ISettingsFacade>();
            settingsFacadeMock.SetupGet(x => x.IsBackupAutouploadEnabled).Returns(true);

            var taskStopped = false;
            var backgroundTaskManager = new Mock<IBackgroundTaskManager>();
            backgroundTaskManager.Setup(x => x.StopBackupSyncTask()).Callback(() => taskStopped = true);

            // Act
            var vm = new SettingsBackgroundJobViewModel(settingsFacadeMock.Object, backgroundTaskManager.Object,
                new Mock<IMvxLogProvider>().Object,
                new Mock<IMvxNavigationService>().Object) {IsAutoBackupEnabled = false};

            // Assert
            Assert.True(taskStopped);
        }
    }
}
