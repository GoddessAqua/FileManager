using FileManager.DTO;
using System;
using System.Collections;
using System.Windows.Forms;

namespace FileManager.Services
{
    internal class ListViewItemComparerService : IComparer
    {
        readonly int _col;
        readonly SortOrder _order;

        public ListViewItemComparerService()
        {
            _col = 0;
            _order = SortOrder.Ascending;
        }

        public ListViewItemComparerService(int column, SortOrder order)
        {
            _col = column;
            _order = order;
        }

        public int Compare(object x, object y)
        {
            try
            {
                int returnVal;
                switch (_col)
                {
                    case (int)ColumnsNames.Name:
                    case (int)ColumnsNames.Type:
                    default:
                        {
                            var firstValue = ((ListViewItem)x).SubItems[_col].Text;
                            var secondValue = ((ListViewItem)y).SubItems[_col].Text;
                            returnVal = firstValue.CompareTo(secondValue);
                            break;
                        }
                    case (int)ColumnsNames.Size:
                        {
                            var firstSizeValue = ((ListViewItem)x).SubItems[_col].Text.Split(" ");
                            var secondSizeValue = ((ListViewItem)y).SubItems[_col].Text.Split(" ");
                            
                            var firstSize = int.Parse(firstSizeValue[0]);
                            var secondSize = int.Parse(secondSizeValue[0]);

                            var firstMeasure = firstSizeValue[1];
                            var secondMeasure = secondSizeValue[1];

                            var firstObj = new SizeDto(firstSize, firstMeasure);
                            var secondObj = new SizeDto(secondSize, secondMeasure);

                            DirectorySizeCalculationService.CastToKB(firstObj);
                            DirectorySizeCalculationService.CastToKB(secondObj);

                            returnVal = firstObj.Size.CompareTo(secondObj.Size);
                            break;
                        }
                }
                
                if (_order == SortOrder.Descending)
                    returnVal *= -1;
                
                return returnVal;
            }
            catch
            {
                return 0;
            }
        }
    }
}
