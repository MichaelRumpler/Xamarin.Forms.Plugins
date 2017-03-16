using System;
using System.Collections.Generic;

using Xamarin.Forms;
using SampleApp.Pages;

namespace SampleApp
{
	public partial class MainPage : ContentPage
	{
		public MainPage ()
		{
			InitializeComponent ();
			BindingContext = new MainPageViewModel ();
		}

		private async void OnItemSelected (object sender, ItemTappedEventArgs args)
		{
			var selectedItem = args.Item.ToString ();

			switch (selectedItem) {

			case PageTitle.EntriesOnlyPage:
				{
					await Navigation.PushAsync (new EntriesOnly ());
					break;
				}

			case PageTitle.TabbedPage:
				{
					await Navigation.PushAsync (new TabsPage ());
					break;
				}
			case PageTitle.WithOtherContent:
				{
					await Navigation.PushAsync (new WithOtherContent ());
					break;
				}
			case PageTitle.WithScrollView:
				{
					await Navigation.PushAsync (new WithScrollView ());
					break;
				}
			case PageTitle.SearchBar:
				{
					await Navigation.PushAsync (new SearchBarPage ());
					break;
				}
			case PageTitle.Editor:
				{
					await Navigation.PushAsync (new MultiLineText ());
					break;
				}
			case PageTitle.FullScreen:
				{
					await Navigation.PushAsync(new FullScreenEditor ());
					break;
				}
			}
		}
	}
}

