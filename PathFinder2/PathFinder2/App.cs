#region Namespaces
using System;
using System.Collections.Generic;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;
#endregion

namespace PathFinder2
{
    class App : IExternalApplication
    {
        public TextBoxData tbv = new TextBoxData("vertical spacing");
        public TextBoxData tbh = new TextBoxData("horizontal spacing");

        // define a method that will create our tab and button
        public void AddRibbonPanel(UIControlledApplication application)
        {
            String tabName = "AVCG";
            application.CreateRibbonTab(tabName);

            // Create a ribbon panel
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Параметры");
            
            PushButtonData reinButton = new PushButtonData("Path Distributed Reinforcement", "Арм.Стены", @"C:\Users\Павел Русаков\AppData\Roaming\Autodesk\Revit\Addins\2018\PathFinder2.dll", "PathFinder2.Command");
            reinButton.LargeImage = new BitmapImage(new Uri(@"C:\Users\Павел Русаков\Desktop\1.png"));
            panel.AddItem(reinButton);

            tbv.ToolTip = "Шаг арматуры по-вертикали";
            tbh.ToolTip = "Шаг арматуры по-горизонтали";

            IList<RibbonItem> stackedItems = panel.AddStackedItems(tbv, tbh);
            TextBox tBoxVertical = stackedItems[0] as TextBox;
            TextBox tBoxHorizontal = stackedItems[1] as TextBox;
            
            tBoxVertical.EnterPressed += new EventHandler<Autodesk.Revit.UI.Events.TextBoxEnterPressedEventArgs>(ProcessText1);
            tBoxHorizontal.EnterPressed += new EventHandler<Autodesk.Revit.UI.Events.TextBoxEnterPressedEventArgs>(ProcessText2);
           

        }
        void ProcessText1(object sender, Autodesk.Revit.UI.Events.TextBoxEnterPressedEventArgs args)
        {
            // cast sender as TextBox to retrieve text value
            TextBox textBox = sender as TextBox;
            string strText = textBox.Value as string;
            Shared.vSpacing = strText;
        }

        void ProcessText2(object sender, Autodesk.Revit.UI.Events.TextBoxEnterPressedEventArgs args)
        {
            // cast sender as TextBox to retrieve text value
            TextBox textBox = sender as TextBox;
            string strText = textBox.Value as string;
            Shared.hSpacing = strText;
        }


        public Result OnStartup(UIControlledApplication a)
        {
            AddRibbonPanel(a);
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}
