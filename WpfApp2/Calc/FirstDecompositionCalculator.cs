using System;
using System.Linq;
using WpfApp2.DB.Models;

namespace WpfApp2.Calc {

    
    /// <summary>
    /// Класс реализующий все необходимые расчёты для первого и второго уровней декомпозиции
    /// </summary>
    class FirstDecompositionCalculator
    {
        #region Поля
        /// <summary>
        /// Хранит в себе данные о проекте
        /// </summary>
        ProjectData data;

        /// <summary>
        /// Массив марок для расчётов
        /// </summary>
        int[] marksToCalculate;

        #endregion

        #region Конструкторы
        /// <summary>
        /// Создает экземпляр калькулятора для дальнейших расчётов
        /// </summary>
        /// <param name="data">Ссылка на данные о проекте</param>
        /// <param name="marksToCalculate">Массив марок, с которыми должны производиться расчёты. В случае отсутствия расчёт проводится расчёт по всем маркам</param>
        public FirstDecompositionCalculator(ProjectData data, int[] marksToCalculate = null)
        {
            this.data = data;

            if (marksToCalculate == null)
                this.marksToCalculate = data.marks[0].marks.Keys.ToArray();
            else
                this.marksToCalculate = marksToCalculate;

        } 
        #endregion

        #region Публичные методы

        /// <summary>
        /// Вычисляет фазовое занчение М
        /// </summary>
        /// <param name="epochIndex">Индекс эпохи, в которой проводится расчёт</param>
        /// <returns></returns>
        public double calculateM(int epochIndex)
        {
            return (M(epochIndex, 0, 1));
        }

        /// <summary>
        /// Вычисляет верхний предел М
        /// </summary>
        /// <param name="epochIndex">Индекс эпохи, в которой проводится расчёт</param>
        /// <returns></returns>
        public double calculateM_plus(int epochIndex)
        {
            return (M(epochIndex, data.eAccuracy, 1));
        }

        /// <summary>
        /// Вычисялет нижний предел М
        /// </summary>
        /// <param name="epochIndex">Индекс эпохи, в которой проводится расчёт</param>
        /// <returns></returns>
        public double calculateM_minus(int epochIndex)
        {
            return (M(epochIndex, data.eAccuracy, -1));
        }

        /// <summary>
        /// Вычисляет фазовое значение Alpha в секундах
        /// </summary>
        /// <param name="epochIndex">Индекс эпохи, в которой проводится расчёт</param>
        /// <returns></returns>
        public double calculateAlpha(int epochIndex)
        {
            return alpha(H1xH0(epochIndex), calculateM(0), calculateM(epochIndex));
        }

        /// <summary>
        /// Вычисляет верхний предел Alpha в секундах
        /// </summary>
        /// <param name="epochIndex">Индекс эпохи, в которой проводится расчёт</param>
        /// <returns></returns>
        public double calculateAlpha_plus(int epochIndex)
        {
            return (alpha(H1xH0(epochIndex, data.eAccuracy, 1), calculateM_plus(0), calculateM_plus(epochIndex)));
        }

        /// <summary>
        /// Вычисляет нижний предел Alpha в секундах
        /// </summary>
        /// <param name="epochIndex">Индекс эпохи, в которой проводится расчёт</param>
        /// <returns></returns>
        public double calculateAlpha_minus(int epochIndex)
        {
            return (alpha(H1xH0(epochIndex, data.eAccuracy, -1), calculateM_minus(0), calculateM_minus(epochIndex)));
        }

        /// <summary>
        /// Подсчитывает превышение высот марок
        /// </summary>
        /// <param name="epochIndex"> Индекс эпохи, в которой проводится расчёт </param>
        /// <param name="accuracy"> Величина ошибки Е при рассчете (по умолчанию 0) </param>
        /// <param name="dir"> Направление рассчета (для (Н+) = 1, для (Н-) = -1) </param>
        /// <returns> Превышение для эпохи с учетом ошибки </returns>
        public double H1xH0(int epochIndex = 0, double accuracy = 0, int dir = 1)
        {
            double result = 0;

            foreach (int markIndex in marksToCalculate)
            {
                double zeroVal = data.marks[0].marks[markIndex];
                double epochVal = data.marks[epochIndex].marks[markIndex];
                result += (zeroVal + accuracy * dir) * (epochVal + accuracy * dir);
            }

            return result;
        }

        /// <summary>
        /// Подсчет прогнозного значения М
        /// </summary>
        /// <param name="epochIndex">Индекс эпохи, в которой проводится расчёт</param>
        /// <returns>Расчитанное значение М pr</returns>
        public double calculateMPredict(int epochIndex)
        {
            double a = data.aAccuracy;

            if (epochIndex == 0)
            {
                double avgM = averageM();
                double m0 = calculateM(0);

                return a * m0 + (1 - a) * avgM;

            }
            else if (epochIndex < data.epochCount)
            {

                double m = calculateM(epochIndex);
                return a * m + (1 - a) * calculateMPredict(epochIndex - 1);

            }
            else if (epochIndex == data.epochCount)
            {
                double avgM = averageM();
                return a * avgM + (1 - a) * calculateMPredict(epochIndex - 1);
            }
            else
                throw new InvalidOperationException("Пытаемся посчитать то, что считать не надо");

        }

        /// <summary>
        /// Вычисляет прогнозный угол вращения в заданной эпохе
        /// </summary>
        /// <param name="epochIndex">Индекс эпохи для которой производится расчёт</param>
        /// <returns>Прогнозный угол вращения в заданной эпохе</returns>
        public double calculateAlphaPredict(int epochIndex)
        {
            double a = data.aAccuracy;
            if (epochIndex == 0)
            {
                double avgAlpha = averageAlpha();
                double alpha0 = calculateAlpha(0);

                return a * alpha0 + (1 - a) * avgAlpha;

            }
            else if (epochIndex < data.epochCount)
            {

                double alpha = calculateAlpha(epochIndex);
                return a * alpha + (1 - a) * calculateAlphaPredict(epochIndex - 1);

            }
            else if (epochIndex == data.epochCount)
            {
                double avgAlpha = averageAlpha();
                return a * avgAlpha + (1 - a) * calculateAlphaPredict(epochIndex - 1);
            }
            else
                throw new InvalidOperationException("Пытаемся посчитать то, что считать не надо");

        }



        /// <summary>
        /// Проверяет, стабилен ли объект на проверяемой эпохе
        /// </summary>
        /// <param name="epochIndex">Индекс эпохи, в которой проводится расчёт</param>
        /// <returns>Возвращает логический флаг стабилен объект или нет</returns>
        public bool hasStable(int epochIndex)
        {
            return stabilityTheory(epochIndex) >= stabilityPractice(epochIndex);
        }

        #endregion

        #region Вспомогательные методы
        private double stabilityTheory(int epochIndex)
        {
            return Math.Abs(calculateM_plus(epochIndex) - calculateM_minus(epochIndex));
        }

        private double stabilityPractice(int epochIndex)
        {
            return Math.Abs(M(epochIndex) - M(0));
        }

        /// <summary>
        /// Внутренняя функция для подсчета М, М+, М-. 
        /// </summary>
        /// <param name="epochIndex"> принимает порядковый номер эпохи, в рамках которой происходит рассчет (Обязательный параметр) </param>
        /// <param name="accuracy"> принимает значение точности Е. При нуле рассчитывает М. </param>
        /// <param name="direction"> принимает значение направления вычислений М+ и М-. При значении 1 возвращает М+, при -1 возвращает М- </param>
        /// <returns> Величина тректории для заданной эпохи </returns>        
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

        /// <summary>
        /// Вычисляет значение угла вращения объекта по заданным переменным и переводит в угловые секунды
        /// </summary>
        /// <param name="H"> Превышение высот </param>
        /// <param name="M0"> Фазовое значенеи траектории в нулевой эпохе </param>
        /// <param name="M"> Фазовое значенеи траектории</param>
        /// <returns>Угол вращения в секундах</returns>
        private double alpha(double H, double M0, double M)
        {

            double alpha = Math.Acos((H / (M0 * M)) >= 1 ? 1 : (H / (M0 * M)));

            return Math.Round(alpha * (3600 * 180) / Math.PI, 1);

        }



        /// <summary>
        /// Вычисляет среднее арифметическое значение фазовой траектории 
        /// </summary>
        /// <returns> Среднее арифмитическое значение фазовой траектории </returns>
        private double averageM()
        {
            int counter = 0;
            double sum = 0;
            for (int index = 0; index < data.epochCount; index++)
            {
                sum += calculateM(index);
                counter++;
            }

            return sum / data.marks.Count;
        }

        /// <summary>
        /// Вычисляет среднюю арифметическую величину угла вращения
        /// </summary>
        /// <returns> Средний арифмитическоий угол вращения в секундах</returns>
        private double averageAlpha()
        {
            double sum = 0;
            for (int index = 0; index < data.marks.Count; index++)
                sum += calculateAlpha(index);

            return sum / data.marks.Count;
        } 
        #endregion
    }
}
