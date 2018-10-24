using System.Linq;

using LightControls.ControlOptions.ControlGroups.Data;

using UnityEngine;

using MinMaxGradient = UnityEngine.ParticleSystem.MinMaxGradient;

namespace LightControls.ControlOptions.ControlGroups
{
    [System.Serializable]
    public class GradientControlGroup
    {
        public MinMaxGradient MinMaxGradient => gradientData.MinMaxGradient;

        private GradientControlGroupData gradientData;
        private MinMaxTracker minMaxTracker;

        private Color currentValue;
        
        public GradientControlGroup(GradientControlGroupData data)
        {
            gradientData = data;

            minMaxTracker = new MinMaxTracker(data.MinMaxTracker);

            currentValue = EvaluateCurrentColor();
        }

        public bool IncrementColor()
        {
            return IncrementColor(Time.deltaTime);
        }

        public bool IncrementColor(float incrementAmount)
        {
            bool iterationDone = minMaxTracker.IncrementBy(incrementAmount);

            currentValue = EvaluateCurrentColor();

            return iterationDone;
        }

        public Color GetCurrentColor()
        {
            return currentValue;
        }
        
        public Color GetCurrentMinMaxColor()
        {
            Debug.Assert(gradientData.MinMaxGradient.mode == ParticleSystemGradientMode.Gradient);
            
            return gradientData.MinMaxGradient.gradientMax.Evaluate(minMaxTracker.GetCurrentPoint());
        }

        private Color EvaluateCurrentColor()
        {
            switch (gradientData.MinMaxGradient.mode)
            {
                case ParticleSystemGradientMode.Color:
                    return gradientData.MinMaxGradient.color;
                case ParticleSystemGradientMode.TwoColors:
                    return RandomBetweenColors(gradientData.MinMaxGradient.colorMin, gradientData.MinMaxGradient.colorMax);
                case ParticleSystemGradientMode.RandomColor:
                    int index1 = Random.Range(0, gradientData.MinMaxGradient.gradient.colorKeys.Length - 1);
                    int index2 = Mathf.RoundToInt(Mathf.Repeat(index1 + 1, gradientData.MinMaxGradient.gradient.colorKeys.Length));
                    return RandomBetweenColors(gradientData.MinMaxGradient.gradient.Evaluate(gradientData.MinMaxGradient.gradient.colorKeys[index1].time), gradientData.MinMaxGradient.gradient.Evaluate(gradientData.MinMaxGradient.gradient.colorKeys[index2].time));
                case ParticleSystemGradientMode.Gradient:
                    return gradientData.MinMaxGradient.gradient.Evaluate(minMaxTracker.CurrentTime);
                case ParticleSystemGradientMode.TwoGradients:
                    return RandomBetweenColors(gradientData.MinMaxGradient.gradientMin.Evaluate(minMaxTracker.CurrentTime), gradientData.MinMaxGradient.gradientMax.Evaluate(minMaxTracker.CurrentTime));
                default:
                    throw new System.Exception("Unsupported version of Unity");
            }
        }
        
        private Color RandomBetweenColors(Color color1, Color color2)
        {
            float r = Random.Range(Mathf.Min(color1.r, color2.r), Mathf.Max(color1.r, color2.r));
            float b = Random.Range(Mathf.Min(color1.b, color2.b), Mathf.Max(color1.b, color2.b));
            float g = Random.Range(Mathf.Min(color1.g, color2.g), Mathf.Max(color1.g, color2.g));
            float a = Random.Range(Mathf.Min(color1.a, color2.a), Mathf.Max(color1.a, color2.a));

            Color color = new Color(r, g, b, a);

            return color;
        }
    }
}