namespace BuildSystem
{
    /// <summary>
    /// Object Selection UI inetface. Implement this interface to create a custom selection UI
    /// </summary>
    interface IItemSelectionUI 
    {
        void ToggleMenu();

        void ToggleMenu(bool val);

        void CollapseMenu();

        bool isCollapsed();

        void Populatemenu(BuildItemContainer container, ObjectSelector selector);

    }
}
