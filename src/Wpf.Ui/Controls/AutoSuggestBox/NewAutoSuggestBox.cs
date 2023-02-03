﻿using System.Windows;
using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Wpf.Ui.Common;

namespace Wpf.Ui.Controls;

[TemplatePart(Name = ElementTextBox, Type = typeof(System.Windows.Controls.TextBox))]
[TemplatePart(Name = ElementSuggestionsPopup, Type = typeof(System.Windows.Controls.Primitives.Popup))]
[TemplatePart(Name = ElementSuggestionsList, Type = typeof(System.Windows.Controls.ListView))]
public class NewAutoSuggestBox : System.Windows.Controls.ItemsControl
{
    protected const string ElementTextBox = "PART_TextBox";
    protected const string ElementSuggestionsPopup = "PART_SuggestionsPopup";
    protected const string ElementSuggestionsList = "PART_SuggestionsList";

    #region Static properties

    /// <summary>
    /// Property for <see cref="IsSuggestionListOpen"/>.
    /// </summary>
    public static readonly DependencyProperty IsSuggestionListOpenProperty =
        DependencyProperty.Register(nameof(IsSuggestionListOpen), typeof(bool), typeof(NewAutoSuggestBox),
            new PropertyMetadata(false));

    /// <summary>
    /// Property for <see cref="Text"/>.
    /// </summary>
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(NewAutoSuggestBox),
        new PropertyMetadata(string.Empty));

    /// <summary>
    /// Property for <see cref="PlaceholderText"/>.
    /// </summary>
    public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(nameof(PlaceholderText), typeof(string), typeof(NewAutoSuggestBox),
        new PropertyMetadata(string.Empty));

    /// <summary>
    /// Property for <see cref="MaxSuggestionListHeight"/>.
    /// </summary>
    public static readonly DependencyProperty MaxSuggestionListHeightProperty = DependencyProperty.Register(nameof(MaxSuggestionListHeight), typeof(double), typeof(NewAutoSuggestBox),
        new PropertyMetadata(0d));

    /// <summary>
    /// Property for <see cref="Icon"/>.
    /// </summary>
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(SymbolRegular), typeof(NewAutoSuggestBox),
        new PropertyMetadata(SymbolRegular.Empty));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets a Boolean value indicating whether the drop-down portion of the <see cref="AutoSuggestBox"/> is open.
    /// </summary>
    public bool IsSuggestionListOpen
    {
        get => (bool)GetValue(IsSuggestionListOpenProperty);
        set => SetValue(IsSuggestionListOpenProperty, value);
    }

    /// <summary>
    /// Gets or sets the text that is shown in the control
    /// </summary>
    /// <remarks>
    /// This property is not typically set in XAML.
    /// </remarks>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets the placeholder text to be displayed in the control.
    /// </summary>
    /// <remarks>
    /// The placeholder text to be displayed in the control. The default is an empty string.
    /// </remarks>
    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    /// <summary>
    /// Gets or set the maximum height for the drop-down portion of the <see cref="AutoSuggestBox"/> control.
    /// </summary>
    public double MaxSuggestionListHeight
    {
        get => (double)GetValue(MaxSuggestionListHeightProperty);
        set => SetValue(MaxSuggestionListHeightProperty, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public SymbolRegular Icon
    {
        get => (SymbolRegular)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    #endregion

    protected System.Windows.Controls.TextBox TextBox = null!;
    protected Popup SuggestionsPopup = null!;
    protected ListView SuggestionsList = null!;

    private bool _isLostFocus;

    public NewAutoSuggestBox()
    {
        Unloaded += static (sender, _) =>
        {
            var self = (NewAutoSuggestBox)sender;

            self.ReleaseTemplateResources();
        };
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        TextBox = GetTemplateChild<System.Windows.Controls.TextBox>(ElementTextBox);
        SuggestionsPopup = GetTemplateChild<Popup>(ElementSuggestionsPopup);
        SuggestionsList = GetTemplateChild<ListView>(ElementSuggestionsList);

        TextBox.PreviewKeyDown += TextBoxOnPreviewKeyDown;
        TextBox.TextChanged += TextBoxOnTextChanged;
        TextBox.LostKeyboardFocus += TextBoxOnLostKeyboardFocus;

        SuggestionsList.SelectionChanged += SuggestionsListOnSelectionChanged;
        SuggestionsList.PreviewKeyDown += SuggestionsListOnPreviewKeyDown;
        SuggestionsList.LostKeyboardFocus += SuggestionsListOnLostKeyboardFocus;
    }

    protected T GetTemplateChild<T>(string name) where T : DependencyObject
    {
        if (GetTemplateChild(name) is not T dependencyObject)
            throw new ArgumentNullException(name);

        return dependencyObject;
    }

    protected virtual void ReleaseTemplateResources()
    {
        TextBox.PreviewKeyDown -= TextBoxOnPreviewKeyDown;
        TextBox.TextChanged -= TextBoxOnTextChanged;
        TextBox.LostKeyboardFocus -= TextBoxOnLostKeyboardFocus;

        SuggestionsList.SelectionChanged -= SuggestionsListOnSelectionChanged;
        SuggestionsList.PreviewKeyDown -= SuggestionsListOnPreviewKeyDown;
        SuggestionsList.LostKeyboardFocus -= SuggestionsListOnLostKeyboardFocus;
    }

    private void TextBoxOnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Down || !IsSuggestionListOpen)
            return;

        SuggestionsList.Focus();
    }

    private void TextBoxOnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (e.NewFocus is ListView)
            return;

        IsSuggestionListOpen = false;
        _isLostFocus = true;
    }

    private void TextBoxOnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(TextBox.Text))
        {
            IsSuggestionListOpen = false;
            return;
        }

        IsSuggestionListOpen = true;
    }

    private void SuggestionsListOnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (e.NewFocus is ListViewItem)
            return;

        IsSuggestionListOpen = false;
    }

    private void SuggestionsListOnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Enter)
            return;

        OnSelected(SuggestionsList.SelectedItem);
    }

    private void SuggestionsListOnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isLostFocus)
            return;

        OnSelected(e.AddedItems[0]);
    }

    protected virtual void OnSelected(object selectedObj)
    {
        Debug.WriteLine($"Selected element is {selectedObj}");

        _isLostFocus = false;
        IsSuggestionListOpen = false;
    }
}
