InplaceEditBoxLib

WPF/MVVM control to implement a textbox on top of other elements like a
TreeViewItem or ListViewItem (use case: perform in place edit of a displayed item)

Use Case: Edit-In-Place

The edit-in-place text control contained in this project can be used as a base for developing applications where users would like to edit text strings as overlay over the normally displayed string.

The best and well known example of an edit-in-place text control is the textbox overlay that is used for renaming renaming a file or folder in Windows Explorer. The user typically selects an item in a list (listbox, listview, grid) or structure of items (treeview) and renames the item using a textbox overlay (without an additional dialog).

Change of focus (activation of a different window), pressing escapee leads to canceling of the rename process and pressing enter leads to confirmation of the new string.

Features

This edit-in-place control in this project can be used in the collection of any **ItemsControl** (Treeview, ListBox, ListView etc).

More details here:
https://github.com/Dirkster99/InplaceEditBoxLib/blob/master/README.md

* keybinding - Press F2 to rename - Press ESC to cancel renaming

* Context Menu - Click Rename in context Menu to rename an item
* Double Click - Double click the text portion to start renaming

and Handling Errors, such as:

* Renaming with an invalid character (Press ? in Edit Mode to see a pop-up message)
* Attempting to name 2 items with the same name (Name 2 items 'a' should invoke a pop-up message on the 2nd items rename)
* Minimum and Maximum length of a name should between 1 - 254 Characters (naming item with empty string '' should invoke a pop-up message)

### Editing text with Text and DisplayText properties ###

The edit-in-place control has 2 string properties, one is for display (**DisplayText**) and the other (**Text**) string represents the value that should be edited.

This setup enables application developers to show more than just a name in each item. Each item can, for example, display a name and a number by using the **DisplayText** property, while the **Text** property should contain the string that is to be edit.

The confirmation of editing does not change either of the above dependency properties. The edit-in-place control executes instead the command that is bound to the **RenameCommand** dependency property to let the viewmodel adjust all relevant strings.

The view invokes the bound **RenameCommand** and passes the **RenameCommandParameter** as parameter along.