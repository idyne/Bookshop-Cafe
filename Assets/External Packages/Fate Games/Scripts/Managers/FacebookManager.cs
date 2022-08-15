using Facebook.Unity;

namespace FateGames
{
    public static class FacebookManager
    {
        public delegate void CallbackFunction();
        public static void Initialize(CallbackFunction CallbackFunction)
        {
            if (FB.IsInitialized)
            {
                FB.ActivateApp();
                CallbackFunction();
            }
            else
            {
                //Handle FB.Init
                FB.Init(() =>
                {
                    FB.ActivateApp();
                    CallbackFunction();
                });
            }
            
        }
    }

}
