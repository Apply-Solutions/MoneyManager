﻿using System.Collections.Generic;
using AView = Android.Views.View;
using AViewGroup = Android.Views.ViewGroup;

namespace MoneyFox.Droid.Renderer
{
    public static class ViewGroupExtension
    {
        internal static IEnumerable<T> GetChildrenOfType<T>(this AViewGroup self)
                                       where T : AView
        {
            for(var i = 0; i < self.ChildCount; i++)
            {
                AView child = self.GetChildAt(i);
                var typedChild = child as T;

                if(typedChild != null)
                    yield return typedChild;

                if(child is AViewGroup)
                {
                    IEnumerable<T> myChildren = ((AViewGroup)child).GetChildrenOfType<T>();
                    foreach(T nextChild in myChildren)
                    {
                        yield return nextChild;
                    }
                }
            }
        }
    }
}
