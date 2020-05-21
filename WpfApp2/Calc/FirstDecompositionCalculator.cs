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

        public FirstDecompositionCalculator(ProjectData data)
        {
            this.data = data;
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
            return (alpha(H1xH0(epochIndex), calculateM_plus(0), calculateM_plus(epochIndex)));
        }

        public double calculateAlpha_minus(int epochIndex)
        {
            return (alpha(H1xH0(epochIndex), calculateM_minus(0), calculateM_minus(epochIndex)));
        }

        private double round(double val)
        {
            return Math.Round(val, ROUND_DIGITS);
        }

        public double H1xH0(int epochIndex = 0)
        {
            double result = 0;
            foreach(KeyValuePair<int, double> startMark in data.marks[0].marks)
            {
                double targetValue = data.marks[epochIndex].marks[startMark.Key];
                result += startMark.Value * targetValue;
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
            foreach (KeyValuePair<int, double> startMark in data.marks[epochIndex].marks)
                result += (startMark.Value + (accuracy * direction)) * (startMark.Value + (accuracy * direction));
            return Math.Sqrt(result);
        }


        private double alpha(double H, double M0, double M)
        {
            return Math.Acos(H / (M0 * M));
        }


    }
}
