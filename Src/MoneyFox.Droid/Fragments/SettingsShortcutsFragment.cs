﻿using MoneyFox.Shared.ViewModels;

namespace MoneyFox.Droid.Fragments
{
    [Register("moneyfox.droid.fragments.SettingsShortcutsFragment")]
    public class SettingsShortcutsFragment : BaseFragment<SettingsShortcutsViewModel>
    {
        protected override int FragmentId => Resource.Layout.fragment_settings_shortcuts;
    }
}