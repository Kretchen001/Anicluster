using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Anicluster.windows
{
    /// <summary>
    /// Interaktionslogik für AddTagToAnime.xaml
    /// </summary>
    public partial class AddTagToAnime : Window
    {
        public AddTagToAnime()
        {
            InitializeComponent();
        }

        private void DataGridTagList_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            //// get the id per clicked Details-Button
            //int currentRowIndex = dataGridTagList.Items.IndexOf(dataGridTagList.CurrentItem); // begin by 0. Give the rowNumber | NOT THE ID
            //AnimeTag tempTag = new AnimeTag() {
            //    Id = tagRows[currentRowIndex].id,
            //    TagDesignator = tagRows[currentRowIndex].tagDesignator
            //};
            //if (tagRows[currentRowIndex].isChecked) {
            //    tagRows[currentRowIndex].isChecked = false;
            //    selectedTagList.RemoveAll(x => x.Id == tempTag.Id);
            //}
            //else {
            //    tagRows[currentRowIndex].isChecked = true;
            //    if (!selectedTagList.Contains(tempTag)) {
            //        selectedTagList.Add(tempTag);
            //    }
            //}
            //UpdateDataGrid();
        }

        //private void UpdateDataGrid() {
        //    tagRows = tagRows.OrderBy(x => x.tagDesignator).ToList();
        //    dataGridTagList.ItemsSource = null;
        //    dataGridTagList.ItemsSource = tagRows;
        //}
    }
}
