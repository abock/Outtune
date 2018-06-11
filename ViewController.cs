using System;

using Foundation;
using UIKit;
using WebKit;
using CoreGraphics;

namespace OuttuneMail
{
    public partial class ViewController : UIViewController, IWKUIDelegate
    {
        protected ViewController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            // this is the default blue color; OWA does not seem to allow you to change themes,
            // but we will nevertheless fetch this value again from CSS when it's loaded
            UpdateTheme (UIColor.FromRGB (0, 120, 215));

            var webView = new WKWebView (
                CGRect.Empty,
                new WKWebViewConfiguration {
                    UserContentController = new ContentController (this)
                }) {
                TranslatesAutoresizingMaskIntoConstraints = false,
                AllowsBackForwardNavigationGestures = false,
                UIDelegate = this
            };

            View.AddSubview (webView);

            var layoutGuide = View.SafeAreaLayoutGuide;
            webView.LeadingAnchor.ConstraintEqualTo (layoutGuide.LeadingAnchor).Active = true;
            webView.TrailingAnchor.ConstraintEqualTo (layoutGuide.TrailingAnchor).Active = true;
            webView.TopAnchor.ConstraintEqualTo (layoutGuide.TopAnchor).Active = true;
            webView.BottomAnchor.ConstraintEqualTo (layoutGuide.BottomAnchor).Active = true;

            // OWA does not behave well with main-frame scrolling in Safari,
            // so disable it here ... all of the scrolling we care about will
            // come from an overflow div, etc.
            webView.ScrollView.ScrollEnabled = false;
            webView.ScrollView.Bounces = false;

            webView.LoadRequest (new NSUrlRequest (new NSUrl ("https://outlook.office.com/owa")));

            webView.AddGestureRecognizer (new UIScreenEdgePanGestureRecognizer (gesture => {
                if (gesture.State == UIGestureRecognizerState.Ended) {
                    webView.EvaluateJavaScript("_showa_goBack()", (result, error) => { });
                }
            }) { Edges = UIRectEdge.Left });
        }

        public void UpdateTheme (UIColor backgroundColor)
        {
            View.Layer.BackgroundColor = backgroundColor.CGColor;
            SetNeedsStatusBarAppearanceUpdate ();
        }

        public override UIStatusBarStyle PreferredStatusBarStyle ()
        {
            // FIXME: in theory, determine this from the theme background color
            return UIStatusBarStyle.LightContent;
        }

        sealed class ContentController : WKUserContentController, IWKScriptMessageHandler
        {
            readonly ViewController viewController;

            public ContentController (ViewController viewController)
            {
                this.viewController = viewController;

                AddUserScript (
                    new WKUserScript (new NSString (
                        NSData.FromUrl (NSBundle.MainBundle.GetUrlForResource ("Outtune", "js")),
                        NSStringEncoding.UTF8),
                    injectionTime: WKUserScriptInjectionTime.AtDocumentEnd,
                    isForMainFrameOnly: true));

                AddScriptMessageHandler (this, "styleUpdated");
                AddScriptMessageHandler (this, "console");
            }

            public void DidReceiveScriptMessage (WKUserContentController userContentController, WKScriptMessage message)
            {
                switch (message.Name) {
                case "console":
                    Console.WriteLine ("JS <| {0}", message.Body);
                    break;
                case "styleUpdated" when message.Body is NSDictionary style:
                    var backgroundColor = CssColorParser.Parse (style ["topBarBackgroundColor"] as NSString);
                    viewController.UpdateTheme (backgroundColor);
                    break;
                }
            }
        }

        [Export ("webView:createWebViewWithConfiguration:forNavigationAction:windowFeatures:")]
        public WKWebView CreateWebView (
            WKWebView webView,
            WKWebViewConfiguration configuration,
            WKNavigationAction navigationAction,
            WKWindowFeatures windowFeatures)
        {
            UIApplication.SharedApplication.OpenUrl (navigationAction.Request.Url);
            return null;
        }
    }
}