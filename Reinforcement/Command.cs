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
                
                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Arming..");
                    AreaReinforcement rein = AreaReinforcement.Create(doc, wall, curves, majorDirection, defaultAreaReinforcementTypeId, defaultRebarBarTypeId, defaultHookTypeId);
                    //rein.AdditionalTopCoverOffset = double // any number
                    //rein.AdditionalBottomCoverOffset = double // any number
                    //rein.AreaReinforcementType =  // type of additional reinforcement, f.e horizontal ones
                    //rein.DesignOption 
                    //rein.AdditionalBottomCoverOffset = double // any number
                    //var params = rein.GetOrderedParameters() // returns parameters in order
                    //var subs = rein.GetSubelements()  // returns a set of elements inside


                    trans.Commit();
                }
                
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
    }
}
