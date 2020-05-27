using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using WpfApp2.DB.Models;

namespace WpfApp2.Calc {

    

    class FirstDecompositionCalculator
    {
        const int ROUND_DIGITS = 5;

        ProjectData data;
        int[] marksToCalculate;

        public FirstDecompositionCalculator(ProjectData data, int[] marksToCalculate = null)
        {
            this.data = data;

            if (marksToCalculate == null)
                this.marksToCalculate = data.marks[0].marks.Keys.ToArray();
            else
                this.marksToCalculate = marksToCalculate;

        }
         

        public double calculateM(int epochIndex) {
            return (M(epochIndex, 0, 1));
        }

        public double calculateM_plus(int epochIndex){
            return (M(epochIndex, data.eAccuracy, 1));
        }

        public double calculateM_minus(int epochIndex)
        {
            return (M(epochIndex, data.eAccuracy, -1));
        }

        public double calculateAlpha(int epochIndex)
        {
            return alpha(H1xH0(epochIndex),calculateM(0),calculateM(epochIndex));
        }

        public double calculateAlpha_plus(int epochIndex)
        {
            return (alpha(H1xH0(epochIndex, data.eAccuracy, 1), calculateM_plus(0), calculateM_plus(epochIndex)));
        }

        public double calculateAlpha_minus(int epochIndex)
        {
            return (alpha(H1xH0(epochIndex, data.eAccuracy, -1), calculateM_minus(0), calculateM_minus(epochIndex)));
        }

        public double stabilityTheory(int epochIndex)
        {
            return  Math.Abs( calculateM_plus(epochIndex) - calculateM_minus(epochIndex) );
        }

        public double stabilityPractice(int epochIndex)
        {
            return M(epochIndex) - M(0);
        }

        public bool hasStable(int epochIndex)
        {
            return stabilityTheory(epochIndex) >= stabilityPractice(epochIndex);
        }

        public double H1xH0(int epochIndex = 0, double accuracy = 0, int dir = 1)
        {
            double result = 0;

            foreach(int markIndex in marksToCalculate)
            {
                double zeroVal = data.marks[0].marks[markIndex];
                double epochVal = data.marks[epochIndex].marks[markIndex];
                result += (zeroVal + accuracy * dir) * (epochVal + accuracy * dir);
            }

            return result;
        }


        /// <summary>
        /// Внутренняя функция для подсчета М, М+, М-. 
        /// 1 параметр принимает порядковый номер эпохи, в рамках которой происходит рассчет (Обязательный параметр)
        /// 2 параметр принимает значение точности Е. При нуле рассчитывает М.
        /// 3 параметр принимает значение направления вычислений М+ и М-. При значении 1 возвращает М+, при -1 возвращает М-
        /// </summary>

        private double M(int epochIndex, double accuracy = 0, int direction = 1)
        {
            double result = 0;

            foreach (int markIndex in marksToCalculate)
            {
                double epochVal = data.marks[epochIndex].marks[markIndex];
                result += (epochVal + (accuracy * direction)) * (epochVal + (accuracy * direction));
            }

            return Math.Sqrt(result);
        }


        private double alpha(double H, double M0, double M)
        {

            double alpha = Math.Acos((H / (M0 * M)) >= 1 ? 1 : (H / (M0 * M)));

            return (alpha * (3600 * 180) / Math.PI);

        }


        public double calculateMPredict(int epochIndex)
        {
            double a = data.aAccuracy;
            if (epochIndex == 0)
            {
                double avgM = averageM();
                double m0 = calculateM(0);

                return a * m0 + (1 - a) * avgM;

            }
            else if (epochIndex < data.marksCount)
            {

                double m = calculateM(epochIndex);
                return a * m + (1 - a) * calculateMPredict(epochIndex - 1);

            }
            else if (epochIndex >= data.marksCount)
            {
                double avgM = averageM();
                double m = calculateM(epochIndex);
                return a * avgM + (1 - a) * calculateMPredict(epochIndex - 1);
            }
            else
                throw new InvalidOperationException("Пытаемся посчитать то, что считать не надо");

        }

        public double calculateAlphaPredict(int epochIndex)
        {
            double a = data.aAccuracy;
            if (epochIndex == 0)
            {
                double avgAlpha = averageAlpha();
                double alpha0 = calculateAlpha(0);

                return a * alpha0 + (1 - a) * avgAlpha;

            }
            else if (epochIndex < data.marksCount)
            {

                double alpha = calculateAlpha(epochIndex);
                return a * alpha + (1 - a) * calculateAlphaPredict(epochIndex - 1);

            }
            else if (epochIndex >= data.marksCount)
            {
                double avgAlpha = averageAlpha();
                double alpha = calculateAlpha(epochIndex);
                return a * avgAlpha + (1 - a) * calculateAlphaPredict(epochIndex - 1);
            }
            else
                throw new InvalidOperationException("Пытаемся посчитать то, что считать не надо");

        }


        private double averageM()
        {
            int counter = 0;
            double sum = 0;
            for (int index = 0; index < data.marks.Count; index++)
            {
                 sum += calculateM(index);
                counter++;
            }

            return sum / data.marks.Count;
        }

        private double averageAlpha()
        {
            double sum = 0;
            for (int index = 0; index < data.marks.Count; index++)
                sum += calculateAlpha(index);

            return sum / data.marks.Count;
        }
    }
}
