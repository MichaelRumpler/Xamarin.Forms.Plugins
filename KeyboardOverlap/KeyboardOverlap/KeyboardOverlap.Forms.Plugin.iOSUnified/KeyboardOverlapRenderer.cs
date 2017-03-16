using System;
using Xamarin.Forms.Platform.iOS;
using Foundation;
using UIKit;
using Xamarin.Forms;
using CoreGraphics;
using KeyboardOverlap.Forms.Plugin.iOSUnified;
using System.Diagnostics;

[assembly: ExportRenderer (typeof(Page), typeof(KeyboardOverlapRenderer))]
namespace KeyboardOverlap.Forms.Plugin.iOSUnified
{
	[Preserve (AllMembers = true)]
	public class KeyboardOverlapRenderer : PageRenderer
	{
		NSObject _keyboardShowObserver;
		NSObject _keyboardHideObserver;
		private UITextView _activeTextView;
		private nfloat _cursorTop;
		private double _shiftedBy;
		private bool _isKeyboardShown;
		private nfloat _keyboardHeight;

		public static new void Init()
		{
			var now = DateTime.Now;
			Debug.WriteLine ("Keyboard Overlap plugin initialized {0}", now);
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			var page = Element as ContentPage;

			if (page != null) {
				var contentScrollView = page.Content as ScrollView;

				if (contentScrollView != null)
					return;

				RegisterForKeyboardNotifications ();
			}
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);

			UnregisterForKeyboardNotifications ();
		}

		void RegisterForKeyboardNotifications ()
		{
			if (_keyboardShowObserver == null)
				_keyboardShowObserver = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillShowNotification, OnKeyboardShow);
			if (_keyboardHideObserver == null)
				_keyboardHideObserver = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillHideNotification, OnKeyboardHide);
		}

		void UnregisterForKeyboardNotifications ()
		{
			_isKeyboardShown = false;
			if (_keyboardShowObserver != null) {
				NSNotificationCenter.DefaultCenter.RemoveObserver (_keyboardShowObserver);
				_keyboardShowObserver.Dispose ();
				_keyboardShowObserver = null;
			}

			if (_keyboardHideObserver != null) {
				NSNotificationCenter.DefaultCenter.RemoveObserver (_keyboardHideObserver);
				_keyboardHideObserver.Dispose ();
				_keyboardHideObserver = null;
			}
		}

		protected virtual void OnKeyboardShow (NSNotification notification)
		{
			if (!IsViewLoaded || _isKeyboardShown)
				return;

			_isKeyboardShown = true;
			var activeView = View.FindFirstResponder ();

			if (activeView == null)
				return;

			var keyboardFrame = UIKeyboard.FrameEndFromNotification (notification);
			_keyboardHeight = keyboardFrame.Height;

			_shiftedBy = 0;

			var remainingHeight = View.Frame.Height - _keyboardHeight;

			_activeTextView = activeView as UITextView;
			if (_activeTextView != null && _activeTextView.Frame.Height > remainingHeight)
			{
				_cursorTop = -10;
				// special handling for UITextViews which are higher than the remaining height
				_activeTextView.SelectionChanged += TextView_SelectionChanged;
				return;	// the cursor position is not set yet, so I need to return for now
			}

			var isOverlapping = activeView.IsKeyboardOverlapping (View, _keyboardHeight);

			if (isOverlapping) {
				var viewBottom = activeView.GetViewRelativeBottom (View);
				ShiftPageUp (viewBottom);
			}
		}

		private void TextView_SelectionChanged(object sender, EventArgs e)
		{
			var remainingHeight = View.Frame.Height - _keyboardHeight;

			var relativeCursorPos = GetCursorPosition(_activeTextView);
			if (relativeCursorPos.Top == _cursorTop)
				return;
			_cursorTop = relativeCursorPos.Top;

			var activeViewTop = _activeTextView.ConvertPointFromView(View.Frame.Location, View);
			var absoluteCursorBottom = activeViewTop.Y + relativeCursorPos.Bottom;

			var isOverlapping = absoluteCursorBottom >= remainingHeight;
			if (isOverlapping) {
				ShiftPageUp(absoluteCursorBottom);
			}
		}

		private void OnKeyboardHide (NSNotification notification)
		{
			if (!IsViewLoaded)
				return;

			_isKeyboardShown = false;

			if (_activeTextView != null) {
				_activeTextView.SelectionChanged -= TextView_SelectionChanged;
				_activeTextView = null;
			}

			if (_shiftedBy != 0) {
				ShiftPageDown ();
			}
		}

		private void ShiftPageUp (double activeViewBottom)
		{
			var pageFrame = Element.Bounds;

			var remainingHeight = pageFrame.Height - _keyboardHeight;
			var delta =  activeViewBottom - remainingHeight;

			Console.WriteLine($"Shifting page up {delta} pixels");
			var newY = pageFrame.Y - delta;
			_shiftedBy += delta;
			if(_shiftedBy > _keyboardHeight)		// limit to _keyboardHeight
			{
				newY = newY + _shiftedBy - _keyboardHeight;
				_shiftedBy = _keyboardHeight;
			}

			Element.LayoutTo (new Rectangle (pageFrame.X, newY,
				pageFrame.Width, pageFrame.Height));
		}

		private void ShiftPageDown ()
		{
			Console.WriteLine($"Shifting page down {_shiftedBy} pixels");
			var pageFrame = Element.Bounds;

			var newY = pageFrame.Y + _shiftedBy;

			Element.LayoutTo (new Rectangle (pageFrame.X, newY,
				pageFrame.Width, pageFrame.Height));

			_shiftedBy = 0;
		}

		private CGRect GetCursorPosition(UITextView textview)
		{
			var range = textview.SelectedTextRange;
			if (range != null)
				return textview.GetCaretRectForPosition(range.End);

			return textview.Frame;
		}
	}
}