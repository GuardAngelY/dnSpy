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
using System.Linq;
using dnlib.DotNet;
using dnSpy.Contracts.Controls;
using dnSpy.Contracts.Decompiler;
using dnSpy.Contracts.Files;
using dnSpy.Contracts.Images;
using dnSpy.Contracts.Text;
using dnSpy.Contracts.TreeView;

namespace dnSpy.Analyzer.TreeNodes {
	abstract class AnalyzerTreeNodeData : TreeNodeData, IAnalyzerTreeNodeData {
		public override Guid Guid => Guid.Empty;
		public sealed override bool SingleClickExpandsChildren => Context.SingleClickExpandsChildren;
		public IAnalyzerTreeNodeDataContext Context { get; set; }
		protected abstract ImageReference GetIcon(IDotNetImageManager dnImgMgr);
		protected virtual ImageReference? GetExpandedIcon(IDotNetImageManager dnImgMgr) => null;
		public sealed override ImageReference Icon => GetIcon(this.Context.DotNetImageManager);
		public sealed override ImageReference? ExpandedIcon => GetExpandedIcon(this.Context.DotNetImageManager);

		public sealed override object Text {
			get {
				var gen = ColorizedTextElementProvider.Create(Context.SyntaxHighlight);

				var cached = cachedText?.Target;
				if (cached != null)
					return cached;

				Write(gen.Output, Context.Decompiler);

				var text = gen.CreateResultNewFormatter(Context.UseNewRenderer, filterOutNewLines: true);
				cachedText = new WeakReference(text);
				return text;
			}
		}
		WeakReference cachedText;

		protected abstract void Write(ITextColorWriter output, IDecompiler decompiler);
		public sealed override object ToolTip => null;
		public sealed override string ToString() => ToString(Context.Decompiler);

		public string ToString(IDecompiler decompiler) {
			var output = new StringBuilderTextColorOutput();
			Write(output, decompiler);
			return output.ToString();
		}

		public sealed override void OnRefreshUI() => cachedText = null;
		public abstract bool HandleAssemblyListChanged(IDnSpyFile[] removedAssemblies, IDnSpyFile[] addedAssemblies);
		public abstract bool HandleModelUpdated(IDnSpyFile[] files);

		public static void CancelSelfAndChildren(ITreeNodeData node) {
			foreach (var c in node.DescendantsAndSelf()) {
				var id = c as IAsyncCancellable;
				if (id != null)
					id.Cancel();
			}
		}

		public static void HandleAssemblyListChanged(ITreeNode node, IDnSpyFile[] removedAssemblies, IDnSpyFile[] addedAssemblies) {
			var children = node.DataChildren.ToArray();
			for (int i = children.Length - 1; i >= 0; i--) {
				var c = children[i];
				var n = c as IAnalyzerTreeNodeData;
				if (n == null || !n.HandleAssemblyListChanged(removedAssemblies, addedAssemblies)) {
					AnalyzerTreeNodeData.CancelSelfAndChildren(c);
					node.Children.RemoveAt(i);
				}
			}
		}

		public static void HandleModelUpdated(ITreeNode node, IDnSpyFile[] files) {
			var children = node.DataChildren.ToArray();
			for (int i = children.Length - 1; i >= 0; i--) {
				var c = children[i];
				var n = c as IAnalyzerTreeNodeData;
				if (n == null || !n.HandleModelUpdated(files)) {
					AnalyzerTreeNodeData.CancelSelfAndChildren(c);
					node.Children.RemoveAt(i);
				}
			}
		}

		protected IMemberRef GetOriginalCodeLocation(IMemberRef member) {
			// Emulate the original code. Only the C# override returned something other than the input
			if (Context.Decompiler.UniqueGuid != DecompilerConstants.LANGUAGE_CSHARP_ILSPY)
				return member;
			if (!Context.Decompiler.Settings.GetBoolean(DecompilerOptionConstants.AnonymousMethods_GUID))
				return member;
			return Helpers.GetOriginalCodeLocation(member);
		}

		sealed class TheTreeNodeGroup : ITreeNodeGroup {
			public static ITreeNodeGroup Instance = new TheTreeNodeGroup();

			TheTreeNodeGroup() {
			}

			public double Order => 100;

			public int Compare(ITreeNodeData x, ITreeNodeData y) {
				if (x == y)
					return 0;
				var a = x as IAnalyzerTreeNodeData;
				var b = y as IAnalyzerTreeNodeData;
				if (a == null) return -1;
				if (b == null) return 1;
				return StringComparer.OrdinalIgnoreCase.Compare(a.ToString(), b.ToString());
			}
		}

		public override ITreeNodeGroup TreeNodeGroup => TheTreeNodeGroup.Instance;
	}
}
