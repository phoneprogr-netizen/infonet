#if ANDROID
using Android.Util;
#endif

namespace TuaApp.Utils   // puoi usare anche il namespace del progetto
{
    public static class ALog
    {
        public static void Info(string tag, string msg)
        {
#if ANDROID
            Log.Info(tag, msg);
#endif
        }

        public static void Error(string tag, string msg, Exception? ex = null)
        {
#if ANDROID
            Log.Error(tag, ex is null ? msg : $"{msg}\n{ex}");
#endif
        }
    }
}