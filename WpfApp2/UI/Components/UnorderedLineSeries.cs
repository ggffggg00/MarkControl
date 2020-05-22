using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.DataVisualization.Charting;

namespace WpfApp2.UI.Components
{
    public class UnorderedLineSeries : LineSeries
    {
        protected override void UpdateShape()
        {
            double maximum = ActualDependentRangeAxis.GetPlotAreaCoordinate(
                ActualDependentRangeAxis.Range.Maximum).Value;

            Func<DataPoint, Point> PointCreator = dataPoint =>
                new Point(
                    ActualIndependentAxis.GetPlotAreaCoordinate(
                    dataPoint.ActualIndependentValue).Value,
                    maximum - ActualDependentRangeAxis.GetPlotAreaCoordinate(
                    dataPoint.ActualDependentValue).Value);

            IEnumerable<Point> points = Enumerable.Empty<Point>();
            if (CanGraph(maximum))
            {
                // Original implementation performs ordering here
                points = ActiveDataPoints.Select(PointCreator);
            }
            UpdateShapeFromPoints(points);
        }

        bool CanGraph(double value)
        {
            return !double.IsNaN(value) &&
                !double.IsNegativeInfinity(value) &&
                !double.IsPositiveInfinity(value) &&
                !double.IsInfinity(value);
        }
    }
}
