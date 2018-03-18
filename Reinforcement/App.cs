#region Namespaces
using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
#endregion

namespace Reinforcement
{
    class App : IExternalApplication
    {
        // define a method that will create our tab and button
        public void AddRibbonPanel(UIControlledApplication application)
        {
            String tabName = "AVCG";
            application.CreateRibbonTab(tabName);

            // Create a ribbon panel
            RibbonPanel wallAreaReinPanel = application.CreateRibbonPanel(tabName, "����� �� �������");
            RibbonPanel wallPathReinPanel = application.CreateRibbonPanel(tabName, "����� �� ������");
            RibbonPanel rectColumnReinPanel = application.CreateRibbonPanel(tabName, "�����. �������");
            RibbonPanel reinforceAll = application.CreateRibbonPanel(tabName, "���������� ���");

            PushButtonData wallAreaReinButton = new PushButtonData("Area Reinforcement", "�� ������", @"C:\Users\����� �������\AppData\Roaming\Autodesk\Revit\Addins\2018\Reinforcement.dll", "Reinforcement.Command");
            wallAreaReinPanel.AddItem(wallAreaReinButton);

            PushButtonData wallPathReinButton = new PushButtonData("Path Reinforcement", "�� ������", @"C:\Users\����� �������\AppData\Roaming\Autodesk\Revit\Addins\2018\Reinforcement.dll", "Reinforcement.NewPathReinforcementCommand");
            wallPathReinPanel.AddItem(wallPathReinButton);

            PushButtonData columnReinButton = new PushButtonData("Column Reinforcement", "����", @"C:\Users\����� �������\AppData\Roaming\Autodesk\Revit\Addins\2018\Reinforcement.dll", "Reinforcement.ColumnReinCommand");
            rectColumnReinPanel.AddItem(columnReinButton);

            PushButtonData reinforceAllButton = new PushButtonData("Reinforce All", "���������", @"C:\Users\����� �������\AppData\Roaming\Autodesk\Revit\Addins\2018\Reinforcement.dll", "Reinforcement.ReinforceAll");
            reinforceAll.AddItem(reinforceAllButton);

            // List<RibbonItem> projectButtons = new List<RibbonItem>();
            //projectButtons.AddRange(wallAreaReinPanel.AddStackedItems(wallAreaReinButton));

        }

        public Result OnStartup(UIControlledApplication a)
        {
            this.AddRibbonPanel(a);
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}
