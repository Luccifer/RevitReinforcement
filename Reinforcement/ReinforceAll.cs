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
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    public class ReinforceAll : IExternalCommand
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

            IList<Reference> refs = sel.PickObjects(ObjectType.Element, "Please select an element to copy");
           
            foreach (Reference r in refs)
            {
                Element element = doc.GetElement(r.ElementId);

                Debug.Print(element.ToString());
                Debug.Print(element.Name);
             
                AnalyticalModel analytical = element.GetAnalyticalModel() as AnalyticalModel;
                if (null == analytical)
                {
                    throw new Exception("Can't get AnalyticalModel from the selected wall");
                }

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

                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Arming..");
                    FailureHandlingOptions options = trans.GetFailureHandlingOptions();
                    WarningSuppressor preproccessor = new WarningSuppressor();
                    options.SetClearAfterRollback(true);
                    options.SetFailuresPreprocessor(preproccessor);
                    trans.SetFailureHandlingOptions(options);

                    rein = AreaReinforcement.Create(doc, element, curves, majorDirection, defaultAreaReinforcementTypeId, defaultRebarBarTypeId, defaultHookTypeId);
                    rein.AdditionalTopCoverOffset = 0; // any number <--
                    rein.AdditionalBottomCoverOffset = 0; // any number -->

                    trans.SetFailureHandlingOptions(options);
                    trans.Commit();
                    var suppressor = new WarningSuppressor();
                    
                }
            }

            doc.Export()
            return Result.Succeeded;
        }
    }
}

