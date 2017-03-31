﻿using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using HockeyApp.Android;
using HockeyApp.Android.Metrics;
using MoneyFox.Business.ViewModels;
using MoneyFox.Droid.Services;
using MoneyFox.Foundation.Constants;
using MoneyFox.Foundation.Interfaces;
using MvvmCross.Droid.Shared.Caching;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Platform;

namespace MoneyFox.Droid.Activities
{
    [Activity(Label = "Money Fox",
        Icon = "@drawable/ic_launcher",
        Theme = "@style/AppTheme",
        LaunchMode = LaunchMode.SingleTop,
        Name = "moneyfox.droid.activities.MainActivity")]
    public class MainActivity : MvxCachingFragmentCompatActivity<MainViewModel>
    {
        private CustomFragmentInfo currentFragmentInfo;
        public DrawerLayout DrawerLayout;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.activity_main);

#if !DEBUG
            CrashManager.Register(this, ServiceConstants.HOCKEY_APP_DROID_ID);
            MetricsManager.Register(this, Application, ServiceConstants.HOCKEY_APP_DROID_ID);
#endif

            DrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            if (bundle == null)
            {
                ViewModel.ShowMenuAndFirstDetail();
            }

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            if (toolbar != null)
            {
                SetSupportActionBar(toolbar);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);

                var drawerToggle = new MvxActionBarDrawerToggle(
                    this, // host Activity
                    DrawerLayout, // DrawerLayout object
                    toolbar, // nav drawer icon to replace 'Up' caret
                    Resource.String.drawer_open, // "open drawer" description
                    Resource.String.drawer_close // "close drawer" description
                    );

                DrawerLayout.AddDrawerListener(drawerToggle);
                drawerToggle.SyncState();
            }
        }

        public override void OnBeforeFragmentChanging(IMvxCachedFragmentInfo fragmentInfo,
            Android.Support.V4.App.FragmentTransaction transaction)
        {
            var currentFrag = SupportFragmentManager.FindFragmentById(Resource.Id.content_frame) as MvxFragment;

            if (currentFrag != null
                && currentFrag.FindAssociatedViewModelType(typeof(MainActivity)) != fragmentInfo.ViewModelType)
            {
                fragmentInfo.AddToBackStack = true;
            }

            transaction.SetCustomAnimations(Resource.Animation.abc_fade_in,
                Resource.Animation.abc_fade_out);

            base.OnBeforeFragmentChanging(fragmentInfo, transaction);
        }

        public override void OnFragmentChanged(IMvxCachedFragmentInfo fragmentInfo)
        {
            currentFragmentInfo = fragmentInfo as CustomFragmentInfo;
        }

        public override void OnBackPressed()
        {
            if (DrawerLayout != null && DrawerLayout.IsDrawerOpen(GravityCompat.Start))
            {
                DrawerLayout.CloseDrawers();
            }
            else
            {
                base.OnBackPressed();
            }
        }

        /// <summary>
        ///     Handle Clicks in the Toolbar
        /// </summary>
        /// <param name="item">Represents the clicked menu item.</param>
        /// <returns>Returns true if the operation was succesful and false if not.</returns>
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    return HandleHomeButton();
            }
            return base.OnOptionsItemSelected(item);
        }

        private bool HandleHomeButton()
        {
            if (currentFragmentInfo != null && currentFragmentInfo.IsRoot)
            {
                DrawerLayout.OpenDrawer(GravityCompat.Start);
            }
            else
            {
                SupportFragmentManager.PopBackStackImmediate();
            }
            return true;
        }

        public class CustomFragmentInfo : MvxCachedFragmentInfo
        {
            public CustomFragmentInfo(string tag, Type fragmentType, Type viewModelType, bool cacheFragment = true,
                bool isRoot = false)
                : base(tag, fragmentType, viewModelType, cacheFragment, true)
            {
                IsRoot = isRoot;
            }

            public bool IsRoot { get; set; }
        }
    }
}