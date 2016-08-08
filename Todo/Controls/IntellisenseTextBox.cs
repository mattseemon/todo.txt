using Seemon.Todo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Seemon.Todo.Controls
{
    public class IntellisenseTextBox : TextBox
    {
        public static readonly DependencyProperty TaskListProperty = DependencyProperty.Register("TaskList", typeof(TaskList), typeof(IntellisenseTextBox), new UIPropertyMetadata(null));
        public static readonly DependencyProperty CaseSensitiveProperty = DependencyProperty.Register("CaseSensitive", typeof(bool), typeof(IntellisenseTextBox), new UIPropertyMetadata(false));

        private Popup IntellisensePopup { get; set; }
        private ListBox IntellisenseList { get; set; }
        private int IntellisensePosition { get; set; }

        public TaskList TaskList
        {
            get { return (TaskList)GetValue(TaskListProperty); }
            set { SetValue(TaskListProperty, value); }
        }

        public bool CaseSensitive
        {
            get { return (bool)GetValue(CaseSensitiveProperty); }
            set { SetValue(CaseSensitiveProperty, value); }
        }
        
        public IntellisenseTextBox()
        {
            this.IntellisenseList = new ListBox();
            this.IntellisenseList.IsTextSearchEnabled = true;
            this.IntellisenseList.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            this.IntellisenseList.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            this.IntellisenseList.PreviewKeyUp += OnListPreviewKeyUp;
            this.IntellisenseList.MouseUp += OnListMouseUp;

            this.IntellisensePopup = new Popup();
            this.IntellisensePopup.IsOpen = false;
            this.IntellisensePopup.Height = Double.NaN;
            this.IntellisensePopup.MinWidth = 150;
            this.IntellisensePopup.MaxWidth = 500;
            this.IntellisensePopup.StaysOpen = false;
            this.IntellisensePopup.Placement = PlacementMode.Bottom;
            this.IntellisensePopup.PlacementTarget = this;
            this.IntellisensePopup.Child = this.IntellisenseList;

            this.TextChanged += OnTextChanged;
        }

        public void ShowIntellisensePopup(IEnumerable<string> s, Rect placement)
        {
            if (s == null || s.Count() == 0)
                return;

            this.IntellisensePopup.PlacementRectangle = placement;
            this.IntellisenseList.ItemsSource = s;
            this.IntellisenseList.SelectedItem = null;
            this.IntellisensePopup.IsOpen = true;

            this.Focus();
        }

        public void HideIntellisensePopup()
        {
            this.IntellisensePopup.IsOpen = false;
        }

        private void InsertIntellisense()
        {
            HideIntellisensePopup();

            if(this.IntellisenseList.SelectedItem == null)
            {
                this.Focus();
                return;
            }

            this.Text = this.Text.Remove(this.IntellisensePosition, this.CaretIndex - this.IntellisensePosition);

            var newText = this.IntellisenseList.SelectedItem.ToString();
            this.Text = this.Text.Insert(this.IntellisensePosition, newText);
            this.CaretIndex = this.IntellisensePosition + newText.Length;

            this.Focus();
        }

        private string FindIntellisenseWord()
        {
            return this.Text.Substring(this.IntellisensePosition + 1, this.CaretIndex - this.IntellisensePosition - 1);
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.Changes.Count != 1 || e.Changes.First().AddedLength < 1 || this.CaretIndex < 1)
                return;

            if (this.TaskList == null)
                return;

            var lastAddedCharacter = this.Text.Substring(this.CaretIndex - 1, 1);
            switch(lastAddedCharacter)
            {
                case "+":
                    this.IntellisensePosition = this.CaretIndex - 1;
                    ShowIntellisensePopup(this.TaskList.Projects, this.GetRectFromCharacterIndex(this.IntellisensePosition));
                    break;
                case "@":
                    this.IntellisensePosition = this.CaretIndex - 1;
                    ShowIntellisensePopup(this.TaskList.Contexts, this.GetRectFromCharacterIndex(this.IntellisensePosition));
                    break;
            }
        }

        private void OnListMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            InsertIntellisense();
        }

        private void OnListPreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Enter:
                case Key.Tab:
                case Key.Space:
                    InsertIntellisense();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    HideIntellisensePopup();
                    this.CaretIndex = this.Text.Length;
                    this.Focus();
                    e.Handled = true;
                    break;
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if(e.Key == Key.Tab)
            {
                HideIntellisensePopup();
                e.Handled = false;  
            }
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            base.OnPreviewKeyUp(e);

            if (this.IntellisensePopup.IsOpen && !this.IntellisenseList.IsFocused)
            {
                if(this.CaretIndex <= this.IntellisensePosition)
                {
                    HideIntellisensePopup();
                    e.Handled = false;
                    return;
                }

                switch (e.Key)
                {
                    case Key.Down:
                        if(this.IntellisenseList.Items.Count != 0)
                        {
                            this.IntellisenseList.SelectedIndex = 0;
                            var listBoxItem = (ListBoxItem)this.IntellisenseList.ItemContainerGenerator.ContainerFromItem(this.IntellisenseList.SelectedItem);
                            listBoxItem.Focus();
                        }
                        e.Handled = true;
                        break;
                    case Key.Escape:
                        HideIntellisensePopup();
                        e.Handled = true;
                        break;
                    case Key.Space:
                    case Key.Enter:
                        HideIntellisensePopup();
                        e.Handled = false;
                        break;
                    default:
                        var word = FindIntellisenseWord();
                        if (this.CaseSensitive)
                            this.IntellisenseList.Items.Filter = (x) => x.ToString().Contains(word);
                        else
                            this.IntellisenseList.Items.Filter = (x) => (x.ToString().IndexOf(word, StringComparison.CurrentCultureIgnoreCase) >= 0);
                        e.Handled = true;
                        break;

                }
            }
        }        
    }
}
