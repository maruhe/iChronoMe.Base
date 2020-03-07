using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Util;
using Android.Views.InputMethods;
using Android.Widget;

using iChronoMe.Core;
using iChronoMe.Core.Types;

using Java.Lang;

using Xamarin.Essentials;

using static Android.Content.Res.Resources;
using AlertDialog = Android.Support.V7.App.AlertDialog;

namespace iChronoMe.Widgets.AndroidHelpers
{ 
    public static class Tools
    {
        public static Activity HelperContext { get; set; }


        public static void ShowDebugToast(Context context, string text, bool bShowLong = false)
        {
#if DEBUG
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try { Toast.MakeText(context, text, bShowLong ? ToastLength.Long : ToastLength.Short).Show(); } catch { }
            });
#endif
        }

        public static void ShowToast(Context context, string text, bool bShowLong = false)
            => MainThread.BeginInvokeOnMainThread(() =>
            {
                try { Toast.MakeText(context, text, bShowLong ? ToastLength.Long : ToastLength.Short).Show(); } catch { }
            });
        public static void ShowToast(Context context, ICharSequence text, bool bShowLong = false)
            => MainThread.BeginInvokeOnMainThread(() =>
            {
                try { Toast.MakeText(context, text, bShowLong ? ToastLength.Long : ToastLength.Short).Show(); } catch { }
            });
        public static void ShowToast(Context context, int resId, bool bShowLong = false)
            => MainThread.BeginInvokeOnMainThread(() =>
            {
                try { Toast.MakeText(context, resId, bShowLong ? ToastLength.Long : ToastLength.Short).Show(); } catch { }
            });

        private static Task<int> tskScDlg { get { return tcsScDlg == null ? Task.FromResult(-1) : tcsScDlg.Task; } }
        private static TaskCompletionSource<int> tcsScDlg = null;

        public static async Task<int> ShowSingleChoiseDlg(Context context, string title, IListAdapter items, bool bAllowAbort = true)
        {
            if (context == null)
                return -1;

            tcsScDlg = new TaskCompletionSource<int>();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    var builder = new AlertDialog.Builder(context).SetTitle(title);
                    if (bAllowAbort)
                        builder = builder.SetNegativeButton(context.Resources.GetString(context.Resources.GetIdentifier("action_abort", "string", "me.ichrono.droid")), (s, e) => { tcsScDlg.TrySetResult(-1); });
                    builder = builder.SetSingleChoiceItems(items, -1, new SingleChoiceClickListener(tcsScDlg));
                    builder = builder.SetOnCancelListener(new myDialogCancelListener<int>(tcsScDlg));
                    var dlg = builder.Create();

                    dlg.Show();
                }
                catch
                {
                    tcsScDlg.TrySetResult(-1);
                }
            });
            await tskScDlg;
            return tskScDlg.Result;
        }

        private class myDialogCancelListener<T> : Java.Lang.Object, IDialogInterfaceOnCancelListener
        {
            TaskCompletionSource<T> Handler;

            public myDialogCancelListener(TaskCompletionSource<T> tcs)
            {
                Handler = tcs;
            }

            public void OnCancel(IDialogInterface dialog)
            {
                if (typeof(T) == typeof(bool))
                    (Handler as TaskCompletionSource<bool>).TrySetResult(false);
                else if (typeof(T) == typeof(int))
                    (Handler as TaskCompletionSource<int>).TrySetResult(-1);
                else if (typeof(string) == typeof(string))
                    (Handler as TaskCompletionSource<string>).TrySetResult(null);
                else if (typeof(object) == typeof(object))
                    (Handler as TaskCompletionSource<object>).TrySetResult(null);
                else
                    Handler.TrySetResult(default(T));

                dialog?.Dismiss();
            }

            protected override void Dispose(bool disposing)
            {
                Handler = null;
                base.Dispose(disposing);
            }
        }

        public class SingleChoiceClickListener : Java.Lang.Object, IDialogInterfaceOnClickListener
        {
            TaskCompletionSource<int> Handler;

            public SingleChoiceClickListener(TaskCompletionSource<int> tcs)
            {
                Handler = tcs;
            }

            public new void Dispose()
            {
                Handler = null;
                base.Dispose();
            }

            public void OnClick(IDialogInterface dialog, int which)
            {
                if (dialog != null)
                    dialog.Dismiss();
                Handler.TrySetResult(which);
            }
        }
    }
}