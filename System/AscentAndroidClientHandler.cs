using Javax.Net.Ssl;
using Xamarin.Android.Net;
using System.Threading.Tasks;
using System.Net.Http;

namespace AscentSolutions.ZebraRfid
{
    public class AscentAndroidClientHandler: AndroidClientHandler
    {
        TlsSSLSocketFactory _customTlsSSLSocketFactory = new TlsSSLSocketFactory();
        protected override Task SetupRequest(HttpRequestMessage request, Java.Net.HttpURLConnection conn)
        {
            if (conn is HttpsURLConnection sslConn)
            {
                if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Lollipop)
                    //Enable support for TLS v1.2 through custom TLS socketfactory
                    sslConn.SSLSocketFactory = _customTlsSSLSocketFactory;
            }
            return base.SetupRequest(request, conn);
        }


        private class TlsSSLSocketFactory : SSLSocketFactory
        {
            readonly SSLSocketFactory factory = (SSLSocketFactory)Default;

            public override string[] GetDefaultCipherSuites()
            {
                return factory.GetDefaultCipherSuites();
            }

            public override string[] GetSupportedCipherSuites()
            {
                return factory.GetSupportedCipherSuites();
            }
            public override Java.Net.Socket CreateSocket(Java.Net.InetAddress address, int port, Java.Net.InetAddress localAddress, int localPort)
            {
                return EnableTlsOnSocket(factory.CreateSocket(address, port, localAddress, localPort));
            }

            public override Java.Net.Socket CreateSocket(Java.Net.InetAddress host, int port)
            {
                return EnableTlsOnSocket(factory.CreateSocket(host, port));
            }

            public override Java.Net.Socket CreateSocket(string host, int port, Java.Net.InetAddress localHost, int localPort)
            {
                return EnableTlsOnSocket(factory.CreateSocket(host, port, localHost, localPort));
            }

            public override Java.Net.Socket CreateSocket(string host, int port)
            {
                return EnableTlsOnSocket(factory.CreateSocket(host, port));
            }

            public override Java.Net.Socket CreateSocket(Java.Net.Socket s, string host, int port, bool autoClose)
            {
                return EnableTlsOnSocket(factory.CreateSocket(s, host, port, autoClose));
            }

            public override Java.Net.Socket CreateSocket()
            {
                return EnableTlsOnSocket(factory.CreateSocket());
            }

            private Java.Net.Socket EnableTlsOnSocket(Java.Net.Socket socket)
            {
                if (socket != null && (socket is SSLSocket sslSocket))
                {
                    sslSocket.SetEnabledProtocols(new string[]{"TLSv1.2"});
                }
                return socket;
            }

            protected override void Dispose(bool disposing)
            {
                factory.Dispose();
                base.Dispose(disposing);
            }
        }
    }
}