using System;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace FFmpegGUI.ScriptableUI
{
    internal class IntegerInput
    {
        private TextBox _input;
        private Button _buttonIncrease;
        private Button _buttonDecrease;

        public Action<int> OnValueChanged;

        private int _min = 0;
        private int _max = 100000;

        public IntegerInput(Grid integerElement, int initial = 0, int min = 0, int max = 100000)
        {
            UIElementCollection children = integerElement.Children;

            _input = (TextBox)children[0];
            _buttonIncrease = (Button)children[2];
            _buttonDecrease = (Button)children[3];

            _min = min;
            _max = max;
            MinLimit(ref initial);
            MaxLimit(ref initial);
            _input.Text = initial.ToString();

            _input.TextChanged += TextChanged;
            _buttonIncrease.Click += Increase;
            _buttonDecrease.Click += Decrease;
        }

        public void Load(int value) 
        {
            MinLimit(ref value);
            MaxLimit(ref value);
            _input.Text = value.ToString();
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            int input = int.Parse(GetInteger(_input.Text));
            MinLimit(ref input);
            MaxLimit(ref input);

            _input.Text = input.ToString();
            OnValueChanged.Invoke(input);
        }

        private void Increase(object sender, System.Windows.RoutedEventArgs e)
        {
            int input = int.Parse(GetInteger(_input.Text));
            input += 1;

            MaxLimit(ref input);

            _input.Text = input.ToString();
            OnValueChanged.Invoke(input);
        }

        private void Decrease(object sender, System.Windows.RoutedEventArgs e)
        {
            int input = int.Parse(GetInteger(_input.Text));
            input -= 1;

            MinLimit(ref input);

            _input.Text = input.ToString();
            OnValueChanged.Invoke(input);
        }

        private void MinLimit(ref int input)
        {
            if (input < _min)
                input = _min;
        }

        private void MaxLimit(ref int input)
        {
            if (input > _max)
                input = _max;
        }

        private string GetInteger(string source)
        {
            foreach (char c in source)
                if (c != '1' && c != '2' && c != '3' && c != '4' && c != '5' && c != '6' && c != '7' && c != '8' && c != '9' && c != '0')
                    source = _min.ToString();

            if (source.Length == 0)
                source = _min.ToString();
                
            return source;
        }
    }
}
