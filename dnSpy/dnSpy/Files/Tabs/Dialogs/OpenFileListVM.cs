﻿/*
    Copyright (C) 2014-2016 de4dot@gmail.com

    This file is part of dnSpy

    dnSpy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    dnSpy is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with dnSpy.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using dnSpy.Contracts.MVVM;
using dnSpy.Properties;

namespace dnSpy.Files.Tabs.Dialogs {
	sealed class OpenFileListVM : ViewModelBase, IDisposable {
		public ICommand RemoveCommand => new RelayCommand(a => Remove(), a => CanRemove);
		public ICommand CreateListCommand => new RelayCommand(a => CreateList(), a => CanCreateList);
		public ObservableCollection<FileListVM> Collection => fileListColl;
		public ICollectionView CollectionView => collectionView;
		public bool SyntaxHighlight { get; }

		readonly ObservableCollection<FileListVM> fileListColl;
		readonly ListCollectionView collectionView;

		public object SelectedItem {
			get { return selectedItem; }
			set {
				if (selectedItem != value) {
					selectedItem = value;
					OnPropertyChanged(nameof(SelectedItem));
				}
			}
		}
		object selectedItem;

		public FileListVM[] SelectedItems {
			get { return selectedItems; }
			set { selectedItems = value ?? Array.Empty<FileListVM>(); }
		}
		FileListVM[] selectedItems;

		public bool SearchingForDefaultLists {
			get { return searchingForDefaultLists; }
			set {
				if (searchingForDefaultLists != value) {
					searchingForDefaultLists = value;
					OnPropertyChanged(nameof(SearchingForDefaultLists));
					OnPropertyChanged(nameof(NotSearchingForDefaultLists));
				}
			}
		}
		bool searchingForDefaultLists;

		public bool NotSearchingForDefaultLists => !SearchingForDefaultLists;

		public string SearchText {
			get { return searchText; }
			set {
				if (searchText != value) {
					searchText = value;
					OnPropertyChanged(nameof(SearchText));
					Refilter();
				}
			}
		}
		string searchText;

		public bool ShowSavedLists {
			get { return showSavedLists; }
			set {
				if (showSavedLists != value) {
					showSavedLists = value;
					OnPropertyChanged(nameof(ShowSavedLists));
					Refilter();
				}
			}
		}
		bool showSavedLists;

		readonly FileListManager fileListManager;
		readonly HashSet<FileListVM> removedFileLists;
		readonly List<FileListVM> addedFileLists;
		readonly Func<string, string> askUser;
		readonly CancellationTokenSource cancellationTokenSource;
		readonly CancellationToken cancellationToken;

		public OpenFileListVM(bool syntaxHighlight, FileListManager fileListManager, Func<string, string> askUser) {
			this.SyntaxHighlight = syntaxHighlight;
			this.fileListManager = fileListManager;
			this.askUser = askUser;
			this.fileListColl = new ObservableCollection<FileListVM>();
			this.collectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(fileListColl);
			this.collectionView.CustomSort = new FileListVM_Comparer();
			this.selectedItems = Array.Empty<FileListVM>();
			this.removedFileLists = new HashSet<FileListVM>();
			this.addedFileLists = new List<FileListVM>();
			this.cancellationTokenSource = new CancellationTokenSource();
			this.cancellationToken = cancellationTokenSource.Token;
			this.searchingForDefaultLists = true;

			var hash = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (var fileList in fileListManager.FileLists) {
				hash.Add(fileList.Name);
				fileListColl.Add(new FileListVM(this, fileList, true, true));
			}
			Refilter();

			Task.Factory.StartNew(() => new DefaultFileListFinder(cancellationToken).Find(), cancellationToken)
			.ContinueWith(t => {
				var ex = t.Exception;
				SearchingForDefaultLists = false;
				if (!t.IsCanceled && !t.IsFaulted) {
					foreach (var defaultList in t.Result) {
						if (hash.Contains(defaultList.Name))
							continue;
						var fileList = new FileList(defaultList);
						fileListColl.Add(new FileListVM(this, fileList, false, false));
					}
					Refilter();
				}
			}, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
		}

		void Refilter() {
			var text = (searchText ?? string.Empty).Trim().ToUpperInvariant();
			if (text == string.Empty && !ShowSavedLists)
				CollectionView.Filter = null;
			else
				CollectionView.Filter = o => CalculateIsVisible((FileListVM)o, text);
		}

		bool CalculateIsVisible(FileListVM vm, string filterText) {
			Debug.Assert(filterText != null && filterText.Trim().ToUpperInvariant() == filterText);
			if (string.IsNullOrEmpty(filterText) && !ShowSavedLists)
				return true;
			if (ShowSavedLists && !vm.IsUserList)
				return false;
			var name = vm.Name.ToUpperInvariant();
			foreach (var s in filterText.ToUpperInvariant().Split(sep)) {
				if (!name.Contains(s))
					return false;
			}
			return true;
		}
		static readonly char[] sep = new char[] { ' ' };

		public bool CanRemove => SelectedItems.Length > 0;

		public void Remove() {
			if (!CanRemove)
				return;
			foreach (var vm in SelectedItems.ToArray()) {
				if (vm.IsExistingList)
					removedFileLists.Add(vm);
				fileListColl.Remove(vm);
			}
		}

		bool CanCreateList => true;

		void CreateList() {
			if (!CanCreateList)
				return;
			var name = askUser(dnSpy_Resources.OpenList_AskForName);
			if (string.IsNullOrEmpty(name))
				return;

			var vm = new FileListVM(this, new FileList(name), false, true);
			addedFileLists.Add(vm);
			fileListColl.Add(vm);
		}

		public void Save() {
			foreach (var removed in removedFileLists) {
				Debug.Assert(removed.IsExistingList);
				if (!removed.IsExistingList)
					continue;
				bool b = fileListManager.SelectedFileList == removed.FileList;
				if (b)
					continue;
				fileListManager.Remove(removed.FileList);
			}
			foreach (var added in addedFileLists) {
				if (removedFileLists.Contains(added))
					continue;
				fileListManager.Add(added.FileList);
			}
		}

		public void Dispose() {
			if (disposed)
				return;
			disposed = true;
			cancellationTokenSource.Cancel();
			cancellationTokenSource.Dispose();
		}
		bool disposed;
	}

	sealed class FileListVM_Comparer : System.Collections.IComparer {
		public int Compare(object x, object y) {
			var a = x as FileListVM;
			var b = y as FileListVM;
			if (a == b)
				return 0;
			if (a == null)
				return -1;
			if (b == null)
				return 1;
			return StringComparer.OrdinalIgnoreCase.Compare(a.Name, b.Name);
		}
	}
}
