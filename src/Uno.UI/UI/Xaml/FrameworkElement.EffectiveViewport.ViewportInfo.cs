﻿#nullable enable

using System;
using System.Linq;
using Windows.Foundation;
using Uno.Extensions;

namespace Windows.UI.Xaml
{
	// Internal interface used to allow communication between the real FrameworkElement
	// and presenters that are only implementing the IFrameworkElement interface (cf. FrameworkElementMixins.tt).
	// It must not be used anywhere else out of the file.
	internal interface IFrameworkElement_EffectiveViewport
	{
		void InitializeEffectiveViewport();
		IDisposable RequestViewportUpdates(IFrameworkElement_EffectiveViewport? child = null);
		void OnParentViewportChanged(IFrameworkElement_EffectiveViewport parent, ViewportInfo viewport, bool isInitial = false);
		void OnLayoutUpdated();
	}

	internal struct ViewportInfo : IEquatable<ViewportInfo>
	{
		public static ViewportInfo Empty { get; } = new ViewportInfo { Effective = Rect.Empty, Clip = Rect.Empty };

		/// <summary>
		/// The element used as reference coordinate space to express the values.
		/// </summary>
		public IFrameworkElement_EffectiveViewport? Reference;

		/// <summary>
		/// The public effective viewport
		/// </summary>
		public Rect Effective;

		/// <summary>
		/// The cumulative clipping applied by all parents scroll ports.
		/// </summary>
		public Rect Clip;

		public ViewportInfo(IFrameworkElement_EffectiveViewport reference, Rect viewport)
		{
			Reference = reference;
			Effective = Clip = viewport;
		}

		public ViewportInfo(IFrameworkElement_EffectiveViewport reference, Rect effective, Rect clip)
		{
			Reference = reference;
			Effective = effective;
			Clip = clip;
		}

		public ViewportInfo GetRelativeTo(IFrameworkElement_EffectiveViewport element)
		{
			Rect effective = Effective, clip = Clip;
			if (element is UIElement uiElement && Reference is UIElement usuallyTheParentOfElement)
			{
				var parentToElement = UIElement.GetTransform(uiElement, usuallyTheParentOfElement).Inverse();

				if (Effective.IsValid)
				{
					effective = parentToElement.Transform(Effective);
				}

				if (Clip.IsValid)
				{
					clip = parentToElement.Transform(Clip);
				}
			}

			// If the Reference or the element is not a UIElement (native), we don't have any RenderTransform,
			// and we assume that the 'element' is stretched in it's parent usually true except for ListView,
			// but we won't even try to support that case.

			return new ViewportInfo(element, effective, clip);
		}

		/// <inheritdoc />
		public override string ToString()
			=> $"{Effective.ToDebugString()} (clip: {Clip.ToDebugString()})";

		/// <inheritdoc />
		public override int GetHashCode()
			=> Effective.GetHashCode();

		/// <inheritdoc />
		public bool Equals(ViewportInfo other)
			=> Equals(this, other);

		/// <inheritdoc />
		public override bool Equals(object obj)
			=> obj is ViewportInfo vp && Equals(this, vp);

		private static bool Equals(ViewportInfo left, ViewportInfo right)
			=> left.Effective.Equals(right.Effective);

		public static bool operator ==(ViewportInfo left, ViewportInfo right)
			=> Equals(left, right);

		public static bool operator !=(ViewportInfo left, ViewportInfo right)
			=> !Equals(left, right);
	}
}