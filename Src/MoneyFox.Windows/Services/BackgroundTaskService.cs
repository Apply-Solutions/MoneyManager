﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using MoneyFox.Shared.Interfaces;

namespace MoneyFox.Windows.Services
{
    public class BackgroundTaskService : IBackgroundTaskService
    {
        private List<TimeTaskConfig> TimeTriggeredTasks => new List<TimeTaskConfig>
        {
            // Task will be executed all 6 hours
            // 360 = 6 * 60 Minutes
            new TimeTaskConfig {Namespace = "MoneyFox.Windows.Tasks", Taskname = "ClearPaymentTask", Interval = 360}
        };

        public async Task RegisterTimeTriggeredTasksAsync()
        {
            foreach (var timeTask in TimeTriggeredTasks)
            {
                if (BackgroundTaskRegistration.AllTasks.Any(task => task.Value.Name == timeTask.Taskname))
                {
                    break;
                }

                await RegisterTimeTaskAsync(timeTask);
            }
        }

        private async Task RegisterTimeTaskAsync(TimeTaskConfig timeTaskConfig)
        {
            var requestAccess = await BackgroundExecutionManager.RequestAccessAsync();

            if (requestAccess == BackgroundAccessStatus.Denied)
            {
                return;
            }

            var taskBuilder = new BackgroundTaskBuilder
            {
                Name = timeTaskConfig.Taskname,
                TaskEntryPoint = string.Format("{0}.{1}", timeTaskConfig.Namespace, timeTaskConfig.Taskname)
            };
            taskBuilder.SetTrigger(new TimeTrigger(timeTaskConfig.Interval, false));

            taskBuilder.Register();
        }

        public struct TimeTaskConfig
        {
            public string Namespace { get; set; }
            public string Taskname { get; set; }
            public uint Interval { get; set; }
        }
    }
}