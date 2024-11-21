
using MauiRfidSample.MVVM.Views;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MauiRfidSample.MVVM.ViewModels
{
	public class PageDataViewModel
	{
		public PageDataViewModel(Type type, string title, string description)
		{
			Type = type;
			Title = title;
			Description = description;
		}

		public Type Type { private set; get; }

		public string Title { private set; get; }

		public string Description { private set; get; }

        [MethodImpl(MethodImplOptions.Synchronized)]
        static PageDataViewModel()
		{
			All = new List<PageDataViewModel>
			{
				new PageDataViewModel(typeof(AssignmentPage), "The App",
									  "Demonstrate features and such"),

                new PageDataViewModel(typeof(ReaderList), "Reader List",
                                      "To select the reader from available reader list"),

			};
		}

		public static IList<PageDataViewModel> All { private set; get; }
	}
}
