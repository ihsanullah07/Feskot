

using UnityEngine;

namespace CaptiveReality.Jni
{
    class Util
    {
        /// <summary>
        /// StaticCall - Call a static Java method in a class using Jni
        /// </summary>
        /// <typeparam name="T">The return type of the method in the class you are calling</typeparam>
        /// <param name="methodName">The name of the method you want to call</param>
        /// <param name="defaultValue">The value you want to return if there was a problem</param>
        /// <param name="androidJavaClass">The name of the Package and Class eg, packagename.myClassName or com.yourandroidlib.example.ClassName</param>
        /// <returns>Generic</returns>
        public static T StaticCall<T>(string methodName, T defaultValue, string androidJavaClass)
        {
            T result;

            // Only works on Android!
            if (Application.platform != RuntimePlatform.Android)
            {
                return defaultValue;
            }

            try
            {
                using (AndroidJavaClass androidClass = new AndroidJavaClass(androidJavaClass))
                {
                    if (null != androidClass)
                    {
                       // result = androidClass.CallStatic<T>(methodName, _port,_ip, _pack);
					     result = androidClass.CallStatic<T>(methodName);
						
                    }
                    else
                    {
                        result = defaultValue;
                    }

                }
            }
            catch (System.Exception ex)
            {
                // If there is an exception, do nothing but return the default value
                // Uncomment this to see exceptions in Unity Debug Log....
                UnityEngine.Debug.Log(string.Format("{0}.{1} Exception:{2}", androidJavaClass, methodName, ex.ToString() ));
                return defaultValue;
            }

            return result;

        }

    }
}