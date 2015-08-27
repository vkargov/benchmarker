﻿using System;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace AndroidAgent
{
	[Activity (Label = "AndroidAgent", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		void SetButtonText (string text)
		{
			Button button = FindViewById<Button> (Resource.Id.myButton);
			button.Text = text;
		}

		void RunBenchmark (string runSetId)
		{
			var task = new Task (() => {
				try {
					Console.WriteLine ("MainActivity | Benchmark : start");

					strcat.Main (new string[] { "10000000" });

					Console.WriteLine ("MainActivity | Benchmark : finished");

					RunOnUiThread (() => {
						SetButtonText ("done");
					});
				} catch (Exception e) {
					Console.WriteLine (e);
				}
			});

			task.Start ();

			Console.WriteLine ("Benchmark started, run set id {0}", runSetId);
		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			Button button = FindViewById<Button> (Resource.Id.myButton);
			
			button.Click += delegate {
				TextView textView = FindViewById<TextView> (Resource.Id.runSetId);
				var runSetId = textView.Text;
				SetButtonText ("running");
				RunBenchmark (runSetId);
			};

			Console.WriteLine ("OnCreate finished");
		}
	}
}