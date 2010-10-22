using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MultiAgentLab.Classes;
using System.Threading;

namespace MultiAgentLab
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        List<Agent> agentsList = new List<Agent>();
        Timer agentTicker;
        Timer newAgentTicker;
        int count = 0;
        Field field;

        public Window1()
        {
            InitializeComponent();
        }

        private void TimerTick(object state)
        {
            var field = state as Field;
            lock (agentsList)
            {
                foreach (var agent in agentsList)
                    agent.Process(field);
            }

            foreach (var row in field)
                foreach (var square in row)
                    if (square.PheremoneLevel > 1)
                        square.PheremoneLevel -= 0.00001;
            count++;
            if (count > 10000)
            {
                agentsList.Add(new Agent(field.StartPoint));
                count = 0;
            }
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            field = new Field(100, 100, @"C:\Users\Tony\Documents\Python Code\sensorData.csv");

            //agentTicker = new Timer(TimerTick, DataContext, 10000, 100);
        }
    }
}
