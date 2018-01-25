#region Frameworks
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
#endregion

namespace Reinforcement
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            #region App Constants and instances
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;
            #endregion

            #region Core Functionality
            
            // Retrieve elements from database
            Reference pickedObj = sel.PickObject(ObjectType.Element, "Please select an element to copy.");
            if (pickedObj != null && pickedObj.ElementId != ElementId.InvalidElementId)
            {
                Element element = doc.GetElement(pickedObj.ElementId);
                Wall wall = element as Wall;

                Debug.Print(wall.Name);

                AnalyticalModel analytical = wall.GetAnalyticalModel() as AnalyticalModel;
                if (null == analytical)
                {
                    throw new Exception("Can't get AnalyticalModel from the selected wall");
                }

                #region Path Reinforcement Implementation
                /*
                List<Curve> curves = new List<Curve>();
                LocationCurve location = wall.Location as LocationCurve;
                XYZ start = location.Curve.GetEndPoint(0);
                XYZ end = location.Curve.GetEndPoint(1);
                curves.Add(Line.CreateBound(start, end));
                ElementId defaultRebarBarTypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.RebarBarType);
                ElementId defaultPathReinforcementTypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.PathReinforcementType);
                ElementId defaultHookTypeId = ElementId.InvalidElementId;

                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Arming..");


                    PathReinforcement rein = PathReinforcement.Create(doc, wall, curves, true, defaultPathReinforcementTypeId, defaultRebarBarTypeId, defaultHookTypeId, defaultHookTypeId);
                    rein.AdditionalOffset = 10;
                    Console.WriteLine(rein.Parameters);
                    Console.WriteLine();
                    Console.WriteLine(rein.ParametersMap);
                    Console.WriteLine();
                    Console.WriteLine(rein.PrimaryBarOrientation);
                    Console.WriteLine();
                    Console.WriteLine(rein.GetOrderedParameters());
                    Console.WriteLine();
                    Console.WriteLine(rein.GetSubelements());
                    Console.WriteLine();
                    
                    trans.Commit();
                }
                */
                #endregion

                #region Area Reinforcement Implementation
                /* Area Reinforce */
                AreaReinforcement rein = null;
                IList<Curve> curves = analytical.GetCurves(AnalyticalCurveType.ActiveCurves);
                Line firstLine = (Line)(curves[0]);
                XYZ majorDirection = 
                    new XYZ( 
                    firstLine.GetEndPoint(1).X - firstLine.GetEndPoint(0).X,
                    firstLine.GetEndPoint(1).Y - firstLine.GetEndPoint(0).Y,
                    firstLine.GetEndPoint(1).Z - firstLine.GetEndPoint(0).Z
                    );
                ElementId defaultRebarBarTypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.RebarBarType);
                ElementId defaultAreaReinforcementTypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.AreaReinforcementType);
                ElementId defaultHookTypeId = ElementId.InvalidElementId;
                IList<Rebar> allRebar = null;

                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Arming..");
                    rein = AreaReinforcement.Create(doc, wall, curves, majorDirection, defaultAreaReinforcementTypeId, defaultRebarBarTypeId, defaultHookTypeId);
                    rein.AdditionalTopCoverOffset = 0; // any number <--
                    rein.AdditionalBottomCoverOffset = 0; // any number -->

                  
                   

                    trans.Commit();
                }

                /*|
                TaskDialog td = new TaskDialog("spacing?");
                td.MainInstruction = "spacing!";
                td.CommonButtons = TaskDialogCommonButtons.Yes;
                td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "yes");
                td.Show();

                using (Transaction trans = new Transaction(doc))
                {
                    IList<Parameter> parameters = rein.GetOrderedParameters(); //14  20 26! 32!
                    var i = 0;
                    trans.Start("Spacing..");
                    foreach (Parameter p in parameters)
                    {

                        Debug.Print(p.Definition.Name);
                        Debug.Print(p.Definition.ParameterType.ToString());
                        Debug.Print(i.ToString());
                        Debug.Print("");

                        if (i == 14)
                        {
                            p.SetValueString("500.0");
                        }

                        if (i == 20)
                        {
                            p.SetValueString("500.0");
                        }

                        if (i == 26)
                        {
                            p.SetValueString("500.0");
                        }

                        if (i == 32)
                        {
                            p.SetValueString("500.0");
                        }

                        Debug.Print(p.Definition.Name);
                        Debug.Print(p.Definition.ParameterType.ToString());
                        Debug.Print(i.ToString());
                        Debug.Print("");
                        i += 1;
                    }
                    trans.Commit();
                }
                */

                #endregion

            }
            #endregion
            
            #region StandartFiltering for Future
            /*
             * FilteredElementCollector col
              = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.INVALID)
                .OfClass(typeof(Wall));
            

            // Filtered element collector is iterable

            foreach (Element e in col)
            {
                Debug.Print(e.Name);
            }
            */
            #endregion

            return Result.Succeeded;
        }

        Rebar CreateRebar(Document document, FamilyInstance column, RebarBarType barType, RebarHookType hookType)
        {
            // Define the rebar geometry information - Line rebar
            LocationPoint location = column.Location as LocationPoint;
            XYZ origin = location.Point;
            XYZ normal = new XYZ(1, 0, 0);
            // create rebar 9' long
            XYZ rebarLineEnd = new XYZ(origin.X, origin.Y, origin.Z + 9);
            Line rebarLine = Line.CreateBound(origin, rebarLineEnd);

            // Create the line rebar
            IList<Curve> curves = new List<Curve>();
            curves.Add(rebarLine);

            Rebar rebar = Rebar.CreateFromCurves(document, RebarStyle.StirrupTie, barType, hookType, hookType,
                                column, origin, curves, RebarHookOrientation.Right, RebarHookOrientation.Left, true, true);

            if (null != rebar)
            {
                // set specific layout for new rebar as fixed number, with 10 bars, distribution path length of 1.5'
                // with bars of the bar set on the same side of the rebar plane as indicated by normal
                // and both first and last bar in the set are shown
                rebar.SetLayoutAsFixedNumber(10, 1.5, true, true, true);
            }

            return rebar;
        }
    }

}
