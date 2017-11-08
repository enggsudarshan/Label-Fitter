using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Arcade
{

  public class LabelFitterExtra
  {
    float angle = 0.00f, current_angle = 0.00f;

    void Func_1(float angle, Rectangle controllerImage, Label_Fitter.LabelPoint labelPoint)
    {
      double degrees = angle * (Math.PI / 180);
      int point = (int)((controllerImage.Height / 2) * (Math.Tan(degrees)));
      Point p = new Point(controllerImage.Width + controllerImage.Location.X, (controllerImage.Height / 2) - point + controllerImage.Location.Y);
      labelPoint.x = p.X;
      labelPoint.y = p.Y;
    }

    void Func_2(float angle, Rectangle controllerImage, Label_Fitter.LabelPoint labelPoint)
    {
      double degrees = angle * (Math.PI / 180);
      int point = 0;
      if(angle == 90)
      {
        point = 0;
      }
      else
      {
        point = (int)((controllerImage.Height / 2) / (Math.Tan(degrees)));
      }
      Point p = new Point(controllerImage.Width / 2 + controllerImage.Location.X + point, controllerImage.Location.Y);
      labelPoint.x = p.X;
      labelPoint.y = p.Y;
    }

    void Func_3(float angle, Rectangle controllerImage, Label_Fitter.LabelPoint labelPoint)
    {
      double degrees = angle * (Math.PI / 180);
      int point = (int)((controllerImage.Height / 2) * (Math.Tan(degrees)));
      Point p = new Point(controllerImage.Location.X, (controllerImage.Height / 2) + point + controllerImage.Location.Y);
      labelPoint.x = p.X;
      labelPoint.y = p.Y;
    }

    void Func_4(float angle, Rectangle controllerImage, Label_Fitter.LabelPoint labelPoint)
    {
      double degrees = angle * (Math.PI / 180);
      int point = (int)((controllerImage.Height / 2) / (Math.Tan(degrees)));
      Point p = new Point(controllerImage.Width / 2 + controllerImage.Location.X - point, controllerImage.Location.Y + controllerImage.Height);
      labelPoint.x = p.X;
      labelPoint.y = p.Y;
    }

    private static float[] calculateThetas(int count, Rectangle controllerImage)
    {
      float[] thetas = { 0f, 0f, 0f };

      thetas[0] = 360.0f;

      // theta1 = arctan(opposite / adjacent)
      thetas[1] = (float)Math.Atan(((float)controllerImage.Height / 2) / ((float)controllerImage.Width / 2));

      // convert to radians: deg * 180 / pi = rad
      thetas[1] = thetas[1] * (float)(180.0f / Math.PI);

      // theta2 = thetaMax - 2*theta1
      thetas[2] = (thetas[0] - 4 * ((float)thetas[1])) / 4;

      return thetas;
    }

    List<Label_Fitter.LabelPoint> generateLabels(List<Label_Fitter.LabelPoint> potentialLabelPoints, Dictionary<string, Label_Fitter.TargetPoint> targetPoints, Rectangle controllerImage)
    {
      float[] thetas = calculateThetas(targetPoints.Count, controllerImage);
      int totalLabels = Math.Max((int)(targetPoints.Count * 1.1f), 6);
      angle = (thetas[0]) / (totalLabels);
      current_angle = 0f;
      
      //foreach(var lbl in targetPoints)
      for(int i = 0; i < totalLabels; i++)
      {
        Label_Fitter.LabelPoint labelPoint =
          new Label_Fitter.LabelPoint()
          {
            width = 20,
            height = 13,
          };

        potentialLabelPoints.Add(labelPoint);

        if(current_angle == 0f)
        {
          Point p = new Point(controllerImage.Width + controllerImage.Location.X, controllerImage.Height / 2 + controllerImage.Location.Y);
          labelPoint.x = p.X;
          labelPoint.y = p.Y;
        }

        if(current_angle <= thetas[1])
        {
          Func_1(current_angle, controllerImage, labelPoint);
        }

        if(current_angle > thetas[1] && current_angle <= (thetas[1] + 2 * thetas[2]))
        {
          Func_2(current_angle, controllerImage, labelPoint);
        }

        if(current_angle > (thetas[1] + 2 * thetas[2]) && current_angle <= (3 * thetas[1] + 2 * thetas[2]))
        {
          Func_3(current_angle, controllerImage, labelPoint);
        }

        if(current_angle > (3 * thetas[1] + 2 * thetas[2]) && current_angle <= (3 * thetas[1] + 4 * thetas[2]))
        {
          Func_4(current_angle, controllerImage, labelPoint);
        }

        if(current_angle > (3 * thetas[1] + 4 * thetas[2]) && current_angle <= thetas[0])
        {
          Func_1(current_angle, controllerImage, labelPoint);
        }

        current_angle = (float)Math.Round((current_angle + angle), 3);
      }
      
      return potentialLabelPoints;
    }



    void Algorithm_Generate_ClosestLabel(List<Label_Fitter.LabelPoint> tempLabelPoints,
      Dictionary<string, Label_Fitter.TargetPoint> targetPoints, Rectangle controllerImage)
    {
      foreach(var lbl in targetPoints)
      {
        Generate_ClosestLabel(lbl.Key, tempLabelPoints, targetPoints, controllerImage);
        //Swap_Labels(lbl.Key, obtained_key, tempLabelPoints);
      }
    }

    void Generate_ClosestLabel(string key, List<Label_Fitter.LabelPoint> potentialLabelPoints, Dictionary<string, Label_Fitter.TargetPoint> targetPoints, Rectangle controllerImage)
    {
      float min_distance = float.MaxValue;
      float distance = 0;
      Point absoluteLoc = new Point(targetPoints[key].x + controllerImage.Location.X, targetPoints[key].y + controllerImage.Location.Y);

      Label_Fitter.LabelPoint shortestLabel = null;

      double x_distance = 0;
      double y_distance = 0;
      
      foreach(var lbl in potentialLabelPoints.Where((l) => l.key == null))
      {
        x_distance = Math.Pow((float)(absoluteLoc.X - lbl.x), 2);
        y_distance = Math.Pow((float)(absoluteLoc.Y - lbl.y), 2);

        distance = (float)Math.Sqrt((x_distance + y_distance));
        if(distance <= min_distance)
        {
          min_distance = distance;
          shortestLabel = lbl;
        }
      }

      if(shortestLabel != null)
      {
        shortestLabel.key = key;
      }

    }

    //string key, string obtained_key
    //int point_index, int label_index
    void Swap_Labels(Label_Fitter.LabelPoint labelA, Label_Fitter.LabelPoint labelB)
    {
      Point temp_point = new Point(0, 0);

      temp_point.X = labelA.x;
      temp_point.Y = labelA.y;

      labelA.x = labelB.x;
      labelA.y = labelB.y;

      labelB.x = temp_point.X;
      labelB.y = temp_point.Y;
    }


    public void Eliminate_Intersecting_LabelPoints(List<Label_Fitter.LabelPoint> tempLabelPoints,
      Dictionary<string, Label_Fitter.TargetPoint> targetPoints, Rectangle controllerImage)
    {
      // current intersect fix attempt
      int current_count = 1;
      // max attempts
      int max_count = 10;

      while(current_count != 0 && max_count != 0)
      {
        current_count = 0;
        foreach(var lbl in tempLabelPoints)
        {
          Point inside_label_1 = new Point(targetPoints[lbl.key].x + controllerImage.Location.X, targetPoints[lbl.key].y + controllerImage.Location.Y);
          Point outside_label_1 = new Point(lbl.x, lbl.y);

          foreach(var lbl2 in tempLabelPoints)
          {
            Point inside_label_2 = new Point(targetPoints[lbl2.key].x + controllerImage.Location.X, targetPoints[lbl2.key].y + controllerImage.Location.Y);

            Point outside_label_2 = new Point(lbl2.x, lbl2.y);

            double A1 = (outside_label_1.Y - inside_label_1.Y);
            double B1 = (inside_label_1.X - outside_label_1.X);
            double C1 = A1 * inside_label_1.X + B1 * inside_label_1.Y;

            double A2 = (outside_label_2.Y - inside_label_2.Y);
            double B2 = (inside_label_2.X - outside_label_2.X);
            double C2 = A2 * inside_label_2.X + B2 * inside_label_2.Y;

            double det = (A1 * B2) - (A2 * B1);


            if(det != 0)
            {
              double x = (B2 * C1 - B1 * C2) / det;
              double y = (A1 * C2 - A2 * C1) / det;

              if(Math.Min(inside_label_1.X, outside_label_1.X) <= x && x <= Math.Max(inside_label_1.X, outside_label_1.X) &&
                  Math.Min(inside_label_1.Y, outside_label_1.Y) <= y && y <= Math.Max(inside_label_1.Y, outside_label_1.Y) &&
                  Math.Min(inside_label_2.X, outside_label_2.X) <= x && x <= Math.Max(inside_label_2.X, outside_label_2.X) &&
                  Math.Min(inside_label_2.Y, outside_label_2.Y) <= y && y <= Math.Max(inside_label_2.Y, outside_label_2.Y))
              {
                Swap_Labels(lbl, lbl2);
                current_count++;
              }
            }
          }
        }
        max_count--;
      }
    }

    void Allign_LabelPointsOnEgdes(List<Label_Fitter.LabelPoint> tempLabelPoints, Rectangle controllerImage)
    {
      foreach(var lbl in tempLabelPoints)
      {
        if(lbl.y == controllerImage.Location.Y)
        {
          lbl.vAlign = ControlsDisplay.Standard.VAlign.BOTTOM;
        }
        else
        {
          lbl.vAlign = ControlsDisplay.Standard.VAlign.TOP;
        }

        if(lbl.x == controllerImage.Location.X)
        {
          lbl.hAlign = ControlsDisplay.Standard.HAlign.LEFT;
        }
        else
        {
          lbl.hAlign = ControlsDisplay.Standard.HAlign.RIGHT;
        }
      }
    }


    void Remove_Unwanted_LabelPoints(List<Label_Fitter.LabelPoint> tempLabelPoints,
      List<Label_Fitter.LabelPoint> potentialLabelPoints, Rectangle controllerImage)
    {
      tempLabelPoints.RemoveAll(temp => !potentialLabelPoints.Any((lp) => temp.key == lp.key));
    }


    public List<Label_Fitter.LabelPoint> fitLabels(Dictionary<string, Label_Fitter.TargetPoint> targetPoints, List<Label_Fitter.LabelPoint> potentialLabelPoints, Rectangle controllerImage)
    {
      List<Label_Fitter.LabelPoint> tempLabelPoints = new List<Label_Fitter.LabelPoint>();

      generateLabels(tempLabelPoints, targetPoints, controllerImage);
      Algorithm_Generate_ClosestLabel(tempLabelPoints, targetPoints, controllerImage);
      Remove_Unwanted_LabelPoints(tempLabelPoints, potentialLabelPoints, controllerImage);
      Eliminate_Intersecting_LabelPoints(tempLabelPoints, targetPoints, controllerImage);
      Allign_LabelPointsOnEgdes(tempLabelPoints, controllerImage);
      return tempLabelPoints;
    }

  }
}
