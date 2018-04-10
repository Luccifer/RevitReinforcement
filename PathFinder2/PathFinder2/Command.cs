#region Namespaces
using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
#endregion

namespace PathFinder2
{
    #region Transaction,Generation,Journaling settings
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    #endregion
    public class Command : IExternalCommand
    {
        double kM = 0;
        double kA = 0;
        double deltaInAxis = 0;

        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            #region Public variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;
            #endregion

            ElementId defaultRebarBarTypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.RebarBarType);
            RebarBarType bType = doc.GetElement(defaultRebarBarTypeId) as RebarBarType;

            double delta = bType.BarDiameter;
            double verSpacing = Convert.ToDouble(Shared.vSpacing);
            double horSpacing = Convert.ToDouble(Shared.hSpacing);


            ElementId hookId = null;
            FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(typeof(RebarHookType));
            foreach (RebarHookType hType in collector)
            {
                if (hType.Id.ToString() == "648153")
                {
                    hookId = hType.Id;
                }
            }

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("First trans");

                if (Shared.vSpacing == null)
                {
                    verSpacing = 400;
                }
                else
                {
                    verSpacing = Convert.ToDouble(Shared.vSpacing);
                }
               
                if (Shared.hSpacing == null)
                {
                    horSpacing = 400;
                }
                else
                {
                    horSpacing = Convert.ToDouble(Shared.hSpacing);
                }

                Reference selectedObject = sel.PickObject(ObjectType.Element, "Please select an element to copy.");
                Element element = doc.GetElement(selectedObject.ElementId);
                
                if (elementIsWall(element))
                {
                    Wall wall = element as Wall;

                    cleanUp(wall);

                    kA = kInAxis(wall);
                    kM = kInMeters(wall);

                    deltaInAxis = getThisValueInAxis(delta, kM) * 1000;
                    double heightInAxis = getThisValueInAxis(getHeightOf(wall), kM);
                    double lengthInAxis = getLengthInAxisOf(wall);

                    IList<XYZ> wallPoints = new List<XYZ>();
                    IList<XYZ> insetPoints = new List<XYZ>();

                    IList<XYZ> wallPointsDelta = new List<XYZ>();
                    IList<XYZ> insetPointsDelta = new List<XYZ>();

                    XYZ rightTopFrontPoint = getRightTopFrontPoint(wall);
                    XYZ leftBottomRearPoint = getLeftBottomRearPoint(wall);

                    for (int i = 0; i <= 7; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                {
                                    XYZ wallRearPoint0 = leftBottomRearPoint;
                                    XYZ deltedWallRearPoint0 = new XYZ(wallRearPoint0.X + deltaInAxis, wallRearPoint0.Y - deltaInAxis, wallRearPoint0.Z);
                                    wallPoints.Add(wallRearPoint0);
                                    wallPointsDelta.Add(deltedWallRearPoint0);
                                    break;
                                }
                            case 1:
                                {
                                    XYZ wallRearPoint1 = new XYZ(leftBottomRearPoint.X + lengthInAxis, leftBottomRearPoint.Y, leftBottomRearPoint.Z);
                                    XYZ deltedWallRearPoint1 = new XYZ(leftBottomRearPoint.X + lengthInAxis - deltaInAxis, leftBottomRearPoint.Y - deltaInAxis, leftBottomRearPoint.Z);
                                    wallPoints.Add(wallRearPoint1);
                                    wallPointsDelta.Add(deltedWallRearPoint1);
                                    break;
                                }
                            case 2:
                                {
                                    XYZ wallRearPoint2 = new XYZ(leftBottomRearPoint.X + lengthInAxis, leftBottomRearPoint.Y, leftBottomRearPoint.Z + heightInAxis);
                                    XYZ deltedWallRearPoint2 = new XYZ(leftBottomRearPoint.X + lengthInAxis - deltaInAxis, leftBottomRearPoint.Y - deltaInAxis, leftBottomRearPoint.Z + heightInAxis);
                                    wallPoints.Add(wallRearPoint2);
                                    wallPointsDelta.Add(deltedWallRearPoint2);
                                    break;
                                }
                            case 3:
                                {
                                    XYZ wallRearPoint3 = new XYZ(leftBottomRearPoint.X, leftBottomRearPoint.Y, leftBottomRearPoint.Z + heightInAxis);
                                    XYZ deltedWallRearPoint3 = new XYZ(leftBottomRearPoint.X + deltaInAxis, leftBottomRearPoint.Y - deltaInAxis, leftBottomRearPoint.Z + heightInAxis);
                                    wallPoints.Add(wallRearPoint3);
                                    wallPointsDelta.Add(deltedWallRearPoint3);
                                    break;
                                }
                            case 4:
                                {
                                    XYZ wallFrontPoint4 = new XYZ(rightTopFrontPoint.X - lengthInAxis, rightTopFrontPoint.Y, rightTopFrontPoint.Z - heightInAxis);
                                    XYZ deltedWallFrontPoint4 = new XYZ(rightTopFrontPoint.X - lengthInAxis + deltaInAxis, rightTopFrontPoint.Y + deltaInAxis, rightTopFrontPoint.Z - heightInAxis);
                                    wallPoints.Add(wallFrontPoint4);
                                    wallPointsDelta.Add(deltedWallFrontPoint4);
                                    break;
                                }
                            case 5:
                                {
                                    XYZ wallFrontPoint5 = new XYZ(rightTopFrontPoint.X, rightTopFrontPoint.Y, rightTopFrontPoint.Z - heightInAxis);
                                    XYZ deltedWallFrontPoint5 = new XYZ(rightTopFrontPoint.X - deltaInAxis, rightTopFrontPoint.Y + deltaInAxis, rightTopFrontPoint.Z - heightInAxis);
                                    wallPoints.Add(wallFrontPoint5);
                                    wallPointsDelta.Add(deltedWallFrontPoint5);
                                    break;
                                }
                            case 6:
                                {
                                    XYZ wallFrontPoint6 = new XYZ(rightTopFrontPoint.X, rightTopFrontPoint.Y, rightTopFrontPoint.Z);
                                    XYZ deltedWallFrontPoint6 = new XYZ(rightTopFrontPoint.X - deltaInAxis, rightTopFrontPoint.Y + deltaInAxis, rightTopFrontPoint.Z);
                                    wallPoints.Add(wallFrontPoint6);
                                    wallPointsDelta.Add(deltedWallFrontPoint6);
                                    break;
                                }
                            case 7:
                                {
                                    XYZ wallFrontPoint7 = new XYZ(rightTopFrontPoint.X - lengthInAxis, rightTopFrontPoint.Y, rightTopFrontPoint.Z);
                                    XYZ deltedWallFrontPoint7 = new XYZ(rightTopFrontPoint.X - lengthInAxis + deltaInAxis, rightTopFrontPoint.Y + deltaInAxis, rightTopFrontPoint.Z);
                                    wallPoints.Add(wallFrontPoint7);
                                    wallPointsDelta.Add(deltedWallFrontPoint7);
                                    break;
                                }
                            default:
                                {
                                    throw new IndexOutOfRangeException();
                                }
                        }
                    }

                    switch (existsAnyInset(wall))
                    {
                        case true:
                            {
                                
                                Element inset = getFirstInsetElement(doc, wall);
                                double insetLengthInAxis = getThisValueInAxis(getLengthOf(inset), kM);
                                double insetHeightInAxis = getThisValueInAxis(getHeightOf(inset), kM);
                                XYZ insetRightFrontTopPoint = getRightTopFrontPoint(inset);
                                XYZ insetLeftRearBottomPoint = getLeftBottomRearPoint(inset);
                                for (int i = 0; i <= 7; i++)
                                {
                                    switch (i)
                                    {
                                        case 0:
                                            {
                                                XYZ insetRearPoint0 = insetLeftRearBottomPoint;
                                                XYZ deltedInsetRearPoint0 = new XYZ(insetLeftRearBottomPoint.X, insetLeftRearBottomPoint.Y - deltaInAxis, insetLeftRearBottomPoint.Z);
                                                insetPoints.Add(insetRearPoint0);
                                                insetPointsDelta.Add(deltedInsetRearPoint0);
                                                break;
                                            }
                                        case 1:
                                            {
                                                XYZ insetRearPoint1 = new XYZ(insetLeftRearBottomPoint.X + insetLengthInAxis, insetLeftRearBottomPoint.Y, insetLeftRearBottomPoint.Z);
                                                XYZ deltedInsetRearPoint1 = new XYZ(insetLeftRearBottomPoint.X + insetLengthInAxis, insetLeftRearBottomPoint.Y - deltaInAxis, insetLeftRearBottomPoint.Z);
                                                insetPoints.Add(insetRearPoint1);
                                                insetPointsDelta.Add(deltedInsetRearPoint1);
                                                break;
                                            }
                                        case 2:
                                            {
                                                XYZ insetRearPoint2 = new XYZ(insetLeftRearBottomPoint.X + insetLengthInAxis, insetLeftRearBottomPoint.Y, insetLeftRearBottomPoint.Z + insetHeightInAxis);
                                                XYZ deltedInsetRearPoint2 = new XYZ(insetLeftRearBottomPoint.X + insetLengthInAxis, insetLeftRearBottomPoint.Y - deltaInAxis, insetLeftRearBottomPoint.Z + insetHeightInAxis);
                                                insetPoints.Add(insetRearPoint2);
                                                insetPointsDelta.Add(deltedInsetRearPoint2);
                                                break;
                                            }
                                        case 3:
                                            {
                                                XYZ insetRearPoint3 = new XYZ(insetLeftRearBottomPoint.X, insetLeftRearBottomPoint.Y, insetLeftRearBottomPoint.Z + insetHeightInAxis);
                                                XYZ deltedInsetRearPoint3 = new XYZ(insetLeftRearBottomPoint.X, insetLeftRearBottomPoint.Y - deltaInAxis, insetLeftRearBottomPoint.Z + insetHeightInAxis);
                                                insetPoints.Add(insetRearPoint3);
                                                insetPointsDelta.Add(deltedInsetRearPoint3);
                                                break;
                                            }
                                        case 4:
                                            {
                                                XYZ insetFrontPoint4 = new XYZ(insetRightFrontTopPoint.X - insetLengthInAxis, insetRightFrontTopPoint.Y, insetRightFrontTopPoint.Z - insetHeightInAxis);
                                                XYZ deltedInsetFrontPoint4 = new XYZ(insetRightFrontTopPoint.X - insetLengthInAxis, insetRightFrontTopPoint.Y + deltaInAxis, insetRightFrontTopPoint.Z - insetHeightInAxis);
                                                insetPoints.Add(insetFrontPoint4);
                                                insetPointsDelta.Add(deltedInsetFrontPoint4);
                                                break;
                                            }
                                        case 5:
                                            {
                                                XYZ insetFrontPoint5 = new XYZ(insetRightFrontTopPoint.X, insetRightFrontTopPoint.Y, insetRightFrontTopPoint.Z - insetHeightInAxis);
                                                XYZ deltedInsetFrontPoint5 = new XYZ(insetRightFrontTopPoint.X, insetRightFrontTopPoint.Y + deltaInAxis, insetRightFrontTopPoint.Z - insetHeightInAxis);
                                                insetPoints.Add(insetFrontPoint5);
                                                insetPointsDelta.Add(deltedInsetFrontPoint5);
                                                break;
                                            }
                                        case 6:
                                            {
                                                XYZ insetFrontPoint6 = new XYZ(insetRightFrontTopPoint.X, insetRightFrontTopPoint.Y, insetRightFrontTopPoint.Z);
                                                XYZ deltedInsetFrontPoint6 = new XYZ(insetRightFrontTopPoint.X, insetRightFrontTopPoint.Y + deltaInAxis, insetRightFrontTopPoint.Z);
                                                insetPoints.Add(insetFrontPoint6);
                                                insetPointsDelta.Add(deltedInsetFrontPoint6);
                                                break;
                                            }
                                        case 7:
                                            {
                                                XYZ insetFrontPoint7 = new XYZ(insetRightFrontTopPoint.X - insetLengthInAxis, insetRightFrontTopPoint.Y, insetRightFrontTopPoint.Z);
                                                XYZ deltedInsetFrontPoint7 = new XYZ(insetRightFrontTopPoint.X - insetLengthInAxis, insetRightFrontTopPoint.Y + deltaInAxis, insetRightFrontTopPoint.Z);
                                                insetPoints.Add(insetFrontPoint7);
                                                insetPointsDelta.Add(deltedInsetFrontPoint7);
                                                break;
                                            }
                                        default:
                                            {
                                                throw new IndexOutOfRangeException();
                                            }
                                    }
                                }

                                double bottomWallToInsetSpace = getThisValueInMeters(insetPointsDelta[0].Z - wallPointsDelta[0].Z, kA);
                                double topWallToInsetSpace = getThisValueInMeters(wallPointsDelta[3].Z - insetPointsDelta[3].Z, kA);
                                double leftWallToInsetSpace = Math.Abs(getThisValueInMeters(wallPointsDelta[0].X - insetPointsDelta[0].X, kA));
                                double rightWallToInsetSpace = Math.Abs(getThisValueInMeters(wallPointsDelta[1].X - insetPointsDelta[1].X, kA));

                                #region rear verticals
                                // left rear wall to left rear inset vertical
                                XYZ point0 = wallPointsDelta[0];
                                XYZ point1 = new XYZ(insetPointsDelta[0].X - getThisValueInAxis(verSpacing, kM), wallPointsDelta[0].Y, wallPointsDelta[0].Z);
                                creatPathReinforcement(doc, wall, point0, point1, null, false, false, 0, true, 500, 0);

                                // bottom rear inset to bottom rear wall vertical
                                XYZ point2 = new XYZ(insetPointsDelta[0].X, wallPointsDelta[0].Y, wallPointsDelta[0].Z);
                                XYZ point3 = new XYZ(insetPointsDelta[1].X, wallPointsDelta[0].Y, wallPointsDelta[0].Z);
                                creatPathReinforcement(doc, wall, point2, point3, null, false, false, 0, true, 500, bottomWallToInsetSpace);

                                // right rear inset to right rear wall vertical 
                                XYZ point4 = new XYZ(insetPointsDelta[1].X + getThisValueInAxis(verSpacing, kM), wallPointsDelta[1].Y, wallPointsDelta[1].Z);
                                XYZ point5 = new XYZ(wallPointsDelta[1].X, wallPointsDelta[1].Y, wallPointsDelta[1].Z);
                                creatPathReinforcement(doc, wall, point4, point5, null, false, false, 0, true, 500, 0);

                                // top rear inset to top rear wall vertical 
                                XYZ point6 = insetPointsDelta[3];
                                XYZ point7 = insetPointsDelta[2];
                                creatPathReinforcement(doc, wall, point6, point7, null, false, false, 0, true, 500, topWallToInsetSpace);

                                #endregion

                                #region front verticals
                                XYZ point8 = wallPointsDelta[4];
                                XYZ point9 = new XYZ(insetPointsDelta[4].X - getThisValueInAxis(verSpacing, kM), wallPointsDelta[4].Y, wallPointsDelta[4].Z);
                                creatPathReinforcement(doc, wall, point8, point9, null, false, false, 0, false, 500, 0);

                                // bottom front inset to bottom front wall vertical
                                XYZ point10 = new XYZ(insetPointsDelta[4].X, wallPointsDelta[4].Y, wallPointsDelta[4].Z);
                                XYZ point11 = new XYZ(insetPointsDelta[5].X, wallPointsDelta[4].Y, wallPointsDelta[4].Z);
                                creatPathReinforcement(doc, wall, point10, point11, null, false, false, 0, false, 500, bottomWallToInsetSpace);

                                // right front inset to right front wall vertical 
                                XYZ point12 = new XYZ(insetPointsDelta[5].X + getThisValueInAxis(verSpacing, kM), wallPointsDelta[5].Y, wallPointsDelta[5].Z);
                                XYZ point13 = new XYZ(wallPointsDelta[5].X, wallPointsDelta[5].Y, wallPointsDelta[5].Z);
                                creatPathReinforcement(doc, wall, point12, point13, null, false, false, 0, false, 500, 0);

                                // top rear inset to top rear wall vertical 
                                XYZ point14 = insetPointsDelta[7];
                                XYZ point15 = insetPointsDelta[6];
                                creatPathReinforcement(doc, wall, point14, point15, null, false, false, 0, false, 500, topWallToInsetSpace);
                                #endregion

                                #region rear horizontal
                                XYZ point16 = wallPointsDelta[0];
                                XYZ point17 = new XYZ(wallPointsDelta[0].X, wallPointsDelta[0].Y, insetPointsDelta[0].Z - getThisValueInAxis(horSpacing, kM));
                                creatPathReinforcement(doc, wall, point17, point16, null, true, false, 0, false, 0, 0);

                                XYZ circlePoint1 = new XYZ(wallPoints[0].X, wallPoints[0].Y, wallPoints[0].Z + deltaInAxis);
                                XYZ circlePoint2 = new XYZ(wallPoints[0].X, wallPoints[0].Y, insetPoints[0].Z - getThisValueInAxis(horSpacing, kM) + deltaInAxis);
                                createPathReinforcementWithHook(doc, wall, circlePoint2, circlePoint1, hookId, false);
                                createPathReinforcementWithHook(doc, wall, circlePoint2, circlePoint1, hookId, true);
                                //-------------------------------------------------------------------------------------------------------------------//

                                XYZ point18 = insetPointsDelta[0];
                                XYZ point19 = insetPointsDelta[3];
                                creatPathReinforcement(doc, wall, point18, point19, null, true, false, 0, false, 0, leftWallToInsetSpace);

                                XYZ circlePoint3 = new XYZ(wallPoints[0].X, wallPoints[0].Y, insetPoints[0].Z + deltaInAxis);
                                XYZ circlePoint4 = new XYZ(wallPoints[3].X, wallPoints[3].Y, insetPoints[3].Z + deltaInAxis);
                                createPathReinforcementWithHook(doc, wall, circlePoint4, circlePoint3, hookId, false);
                                createPathReinforcementWithHook(doc, wall, circlePoint4, circlePoint3, hookId, true);
                                //-------------------------------------------------------------------------------------------------------------------//

                                XYZ point20 = insetPointsDelta[1];
                                XYZ point21 = insetPointsDelta[2];
                                creatPathReinforcement(doc, wall, point21, point20, null, true, false, 0, false, 0, rightWallToInsetSpace);

                                XYZ circlePoint5 = new XYZ(wallPoints[1].X, wallPoints[1].Y, insetPoints[1].Z + deltaInAxis);
                                XYZ circlePoint6 = new XYZ(wallPoints[1].X, wallPoints[1].Y, insetPoints[2].Z + deltaInAxis);
                                createPathReinforcementWithHook(doc, wall, circlePoint5, circlePoint6, hookId, false);
                                createPathReinforcementWithHook(doc, wall, circlePoint5, circlePoint6, hookId, true);
                                //-------------------------------------------------------------------------------------------------------------------//

                                XYZ point22 = new XYZ(wallPointsDelta[3].X, wallPointsDelta[3].Y, insetPointsDelta[3].Z + getThisValueInAxis(horSpacing, kM));
                                XYZ point23 = wallPointsDelta[3];
                                creatPathReinforcement(doc, wall, point23, point22, null, true, false, 0, false, 0, 0);

                                XYZ circlePoint7 = new XYZ(wallPoints[1].X, wallPoints[1].Y, wallPoints[1].Z + deltaInAxis);
                                XYZ circlePoint8 = new XYZ(wallPoints[1].X, wallPoints[1].Y, insetPoints[1].Z - getThisValueInAxis(horSpacing, kM) + deltaInAxis);
                                createPathReinforcementWithHook(doc, wall, circlePoint7, circlePoint8, hookId, false);
                                createPathReinforcementWithHook(doc, wall, circlePoint7, circlePoint8, hookId, true);
                                //-------------------------------------------------------------------------------------------------------------------//

                                XYZ circlePoint9 = new XYZ(wallPoints[2].X, wallPoints[2].Y, wallPoints[2].Z);
                                XYZ circlePoint10 = new XYZ(wallPoints[2].X, wallPoints[2].Y, insetPoints[2].Z + getThisValueInAxis(horSpacing, kM) + deltaInAxis);
                                createPathReinforcementWithHook(doc, wall, circlePoint10, circlePoint9, hookId, false);
                                createPathReinforcementWithHook(doc, wall, circlePoint10, circlePoint9, hookId, true);
                                //-------------------------------------------------------------------------------------------------------------------//
                                XYZ circlePoint11 = new XYZ(wallPoints[3].X, wallPoints[3].Y, wallPoints[3].Z);
                                XYZ circlePoint12 = new XYZ(wallPoints[3].X, wallPoints[3].Y, insetPoints[3].Z + getThisValueInAxis(horSpacing, kM) + deltaInAxis);
                                createPathReinforcementWithHook(doc, wall, circlePoint11, circlePoint12, hookId, false);
                                createPathReinforcementWithHook(doc, wall, circlePoint11, circlePoint12, hookId, true);
                                //-------------------------------------------------------------------------------------------------------------------//
                                #endregion

                                #region front horizontal
                                XYZ point24 = wallPointsDelta[4];
                                XYZ point25 = new XYZ(wallPointsDelta[4].X, wallPointsDelta[4].Y, insetPointsDelta[4].Z - getThisValueInAxis(400, kM));
                                creatPathReinforcement(doc, wall, point25, point24, null, true, false, 0, true, 0, 0);

                                XYZ point26 = insetPointsDelta[4];
                                XYZ point27 = insetPointsDelta[7];
                                creatPathReinforcement(doc, wall, point26, point27, null, true, false, 0, true, 0, leftWallToInsetSpace);

                                XYZ point28 = insetPointsDelta[5];
                                XYZ point29 = insetPointsDelta[6];
                                creatPathReinforcement(doc, wall, point29, point28, null, true, false, 0, true, 0, rightWallToInsetSpace);

                                XYZ point30 = new XYZ(wallPointsDelta[4].X, wallPointsDelta[7].Y, insetPointsDelta[7].Z + getThisValueInAxis(400, kM));
                                XYZ point31 = wallPointsDelta[7];
                                creatPathReinforcement(doc, wall, point31, point30, null, true, false, 0, true, 0, 0);
                                #endregion

                                #region horizontal circled hooks

                                #endregion


                                break;
                            }
                        case false:
                            {
                                //rear horizontal
                                for (double i = wallPoints[3].Z; Math.Abs(i) <= heightInAxis;)
                                {
                                    XYZ point1 = new XYZ(wallPoints[3].X, wallPoints[3].Y, i);
                                    XYZ point2 = new XYZ(wallPoints[3].X, wallPoints[3].Y, i - getThisValueInAxis(400, kM));
                                    XYZ point3 = new XYZ(wallPoints[3].X, wallPoints[3].Y, wallPoints[3].Z - heightInAxis);
                                    if (Math.Abs(i - getThisValueInAxis(400, kM) * 2) > heightInAxis)
                                    {
                                        double spacing = getThisValueInMeters(point1.Z - point3.Z, kA) / 2;
                                        creatPathReinforcement(doc, wall, point1, point3, null, true, true, spacing, true, 0, 0);
                                    }
                                    else
                                    {
                                        creatPathReinforcement(doc, wall, point1, point2, null, true, false, 0, true, 0, 0);
                                    }
                                    i -= getThisValueInAxis(400, kM) * 2;
                                }
                                // front horizontal
                                for (double i = wallPoints[7].Z; Math.Abs(i) <= heightInAxis;)
                                {
                                    XYZ point1 = new XYZ(wallPoints[7].X, wallPoints[7].Y, i);
                                    XYZ point2 = new XYZ(wallPoints[7].X, wallPoints[7].Y, i - getThisValueInAxis(400, kM));
                                    XYZ point3 = new XYZ(wallPoints[7].X, wallPoints[7].Y, wallPoints[7].Z - heightInAxis);
                                    if (Math.Abs(i - getThisValueInAxis(400, kM) * 2) > heightInAxis)
                                    {
                                        double spacing = getThisValueInMeters(point1.Z - point3.Z, kA) / 2;
                                        creatPathReinforcement(doc, wall, point1, point3, null, true, true, spacing, false, 0, 0);
                                    }
                                    else
                                    {
                                        creatPathReinforcement(doc, wall, point1, point2, null, true, false, 0, false, 0, 0);
                                    }
                                    i -= getThisValueInAxis(400, kM) * 2;
                                }

                                // rear vertical
                                for (double i = wallPoints[4].X; i <= wallPoints[5].X;)
                                {
                                    XYZ point1 = new XYZ(i, wallPoints[4].Y, wallPoints[4].Z);
                                    XYZ point2 = new XYZ(i + getThisValueInAxis(400, kM), wallPoints[4].Y, wallPoints[4].Z);
                                    XYZ point3 = new XYZ(wallPoints[5].X, wallPoints[5].Y, wallPoints[5].Z);


                                    if (i + getThisValueInAxis(400, kM) * 2 > wallPoints[5].X)
                                    {
                                        double spacing = getThisValueInMeters(point3.X - point1.X, kA) / 2;
                                        creatPathReinforcement(doc, wall, point1, point3, null, false, true, spacing, true, 500, 0);
                                    }
                                    else
                                    {
                                        creatPathReinforcement(doc, wall, point1, point2, null, false, false, 0, true, 500, 0);
                                    }
                                    i += getThisValueInAxis(400, kM) * 2;
                                }
                                // front vertical
                                for (double i = wallPoints[0].X; i <= wallPoints[1].X;)
                                {
                                    XYZ point1 = new XYZ(i, wallPoints[0].Y, wallPoints[0].Z);
                                    XYZ point2 = new XYZ(i + getThisValueInAxis(400, kM), wallPoints[0].Y, wallPoints[0].Z);
                                    XYZ point3 = new XYZ(wallPoints[1].X, wallPoints[1].Y, wallPoints[1].Z);


                                    if (i + getThisValueInAxis(400, kM) * 2 > wallPoints[1].X)
                                    {
                                        double spacing = getThisValueInMeters(point3.X - point1.X, kA) / 2;
                                        creatPathReinforcement(doc, wall, point1, point3, null, false, true, spacing, false, 500, 0);
                                    }
                                    else
                                    {
                                        creatPathReinforcement(doc, wall, point1, point2, null, false, false, 0, false, 500, 0);
                                    }
                                    i += getThisValueInAxis(400, kM) * 2;
                                }
                                break;
                            }
                    }
                }

                tx.Commit();
            }

            return Result.Succeeded;
        }

        PathReinforcement createPathReinforcementWithHook(Document doc, Wall wall, XYZ point1, XYZ point2, ElementId hookId, bool face)
        {

            List<Curve> curves = new List<Curve>();
            curves.Add(Line.CreateBound(point1, point2));

            ElementId defaultRebarBarTypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.RebarBarType);
            ElementId defaultPathReinforcementTypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.PathReinforcementType);
            ElementId defaultHookTypeId = hookId;

            // Begin to create the path reinforcement
            PathReinforcement rein = PathReinforcement.Create(doc, wall, curves, true, defaultPathReinforcementTypeId, defaultRebarBarTypeId, defaultHookTypeId, defaultHookTypeId);
            if (null == rein)
            {
                throw new Exception("Create path reinforcement failed.");
            }

            IList<Parameter> pars = rein.GetOrderedParameters();
            foreach (Parameter par in pars)
            {
                if (par.Id.ToString() == "-1018307")
                {
                    par.SetValueString("500");
                }
                if (par.Id.ToString() == "-1018301" & face)
                {
                    par.Set(1);
                }
                if (par.Id.ToString() == "-1018302")
                {
                    double var1 = Convert.ToDouble(Shared.hSpacing);
                    double var2 = var1 + getThisValueInMeters(deltaInAxis, kA);
                    par.SetValueString("400");
                }
            }

            return rein;
        }

        PathReinforcement creatPathReinforcement(Document document, Wall wall, XYZ point1, XYZ point2, ElementId hookId, bool horizontal, bool last, double lastSpacing, bool face, double additionalSpaceForVerticalBars, double concreteSpace)
        { 
            if (Shared.hSpacing == null)
            {
                Shared.hSpacing = "400";
            }

            if (Shared.vSpacing == null)
            {
                Shared.vSpacing = "400";
            }

            List<Curve> curves = new List<Curve>();
            curves.Add(Line.CreateBound(point1, point2));

            ElementId defaultRebarBarTypeId = document.GetDefaultElementTypeId(ElementTypeGroup.RebarBarType);
            ElementId defaultPathReinforcementTypeId = document.GetDefaultElementTypeId(ElementTypeGroup.PathReinforcementType);
            ElementId defaultHookTypeId = ElementId.InvalidElementId;
            if (hookId != null)
            {
                defaultHookTypeId = hookId;
            }

            PathReinforcement rein = PathReinforcement.Create(document, wall, curves, true, defaultPathReinforcementTypeId, defaultRebarBarTypeId, defaultHookTypeId, defaultHookTypeId);
            if (null == rein)
            {
                throw new Exception("Create path reinforcement failed.");
            }

            IList<Parameter> parameters = rein.GetOrderedParameters();
            foreach (Parameter par in parameters)
            {
                if (par.Id.ToString() == "-1018301" & face) 
                {
                    par.Set(1);
                }
                if (par.Id.ToString() == "-1018302")
                {
                    if (!last && horizontal)
                    {
                        par.SetValueString(Shared.hSpacing);
                    }
                    else if (!last && !horizontal)
                    {
                        par.SetValueString(Shared.vSpacing);
                    }
                    else
                    {
                        par.SetValueString(lastSpacing.ToString());
                    }
                }
                if (horizontal)
                {
                    if (par.Id.ToString() == "-1018307")
                    {
                        par.SetValueString((getLengthOf(wall) - getThisValueInMeters(deltaInAxis*2, kA)).ToString());
                    }
                }
                else
                {
                    if (par.Id.ToString() == "-1018307")
                    {
                        par.SetValueString((getHeightOf(wall) + additionalSpaceForVerticalBars).ToString());
                    }
                }
                if (concreteSpace != 0)
                {
                    if (par.Id.ToString() == "-1018307")
                    {
                        par.SetValueString(concreteSpace.ToString());
                    }
                }
                if (par.Id.ToString() == "-1018322")
                {
                    par.SetValueString(Convert.ToString(getThisValueInMeters(deltaInAxis, kA)) + "mm");
                }
            
            }

            return rein;
        }

        #region Helpers
        public bool existsAnyInset(Wall wall)
        {
            var ids = wall.FindInserts(true, true, true, true);

            if (ids.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool elementIsWall(Element element)
        {
            if (element is Wall)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void cleanUp(Wall wall)
        {
            IList<Parameter> wallParams = wall.GetOrderedParameters();
            foreach (Parameter par in wallParams)
            {
                if (par.Id.ToString() == "-1013435")
                {
                    par.Set(0);
                }
                if (par.Id.ToString() == "-1013436")
                {
                    par.Set(0);
                }
                if (par.Id.ToString() == "-1013437")
                {
                    par.Set(0);
                }
            }
        }

        public double getLengthOf(Wall wall)
        {
            IList<Parameter> wallParams = wall.GetOrderedParameters();
            double wallLengthInMSystem = 0;
            foreach (Parameter par in wallParams)
            {
                if (par.Id.ToString() == "-1004005")
                {
                    wallLengthInMSystem = Convert.ToDouble(par.AsValueString());
                }
            }
            return wallLengthInMSystem;
        }

        public double getLengthOf(Element element)
        {
            IList<Parameter> pars = element.GetOrderedParameters();
            double length = 0;
            foreach (Parameter par in pars)
            {
                if (par.Id.ToString() == "566502")
                {
                    length = Convert.ToDouble(par.AsValueString());
                }
            }
            return length;
        }

        public double getLengthInAxisOf(Wall wall)
        {
            LocationCurve wallLocation = wall.Location as LocationCurve;
            XYZ pt1 = wallLocation.Curve.GetEndPoint(0);
            XYZ pt2 = wallLocation.Curve.GetEndPoint(1);
            double wallLengthInAxis = Math.Abs(pt2.X - pt1.X);
            return wallLengthInAxis;
        }

        public double getHeightInAxisOf(Wall wall)
        {
            return Math.Abs(wall.get_BoundingBox(null).Max.Z - wall.get_BoundingBox(null).Min.Z);
        }

        public double getHeightOf(Wall wall)
        {
            return (getHeightInAxisOf(wall) * kInMeters(wall));
        }

        public double getHeightOf(Element element)
        {
            IList<Parameter> pars = element.GetOrderedParameters();
            double length = 0;
            foreach (Parameter par in pars)
            {
                if (par.Id.ToString() == "566509")
                {
                    length = Convert.ToDouble(par.AsValueString());
                }
            }
            return length;
        }

        public double kInAxis(Wall wall)
        {
            return getLengthInAxisOf(wall) / getLengthOf(wall);
        }

        public double kInMeters(Wall wall)
        {
            return getLengthOf(wall) / getLengthInAxisOf(wall);
        }

        public double getThisValueInAxis(double number, double kM)
        {
            return number / kM;
        }
    
        public double getThisValueInMeters(double number, double kA)
        {
            return number / kA;
        }

        XYZ getLeftBottomRearPoint(Element element)
        {
            XYZ point = element.get_BoundingBox(null).Min;
            //XYZ point2 = new XYZ(point.X + delta, point.Y + delta, point.Z + delta);
            return point;
        }

        XYZ getRightTopFrontPoint(Element element)
        {
            XYZ point = element.get_BoundingBox(null).Max;
            //XYZ point2 = new XYZ(point.X - delta, point.Y - delta, point.Z - delta);
            return point;
        }

        Element getFirstInsetElement(Document doc, Wall wall)
        {
            return doc.GetElement(wall.FindInserts(true, true, true, true)[0]);
        }
        #endregion
    }
}

