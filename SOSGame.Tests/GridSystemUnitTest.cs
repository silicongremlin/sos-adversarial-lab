using Xunit;
using SOSGame.GUI;
using Avalonia.Controls;
using System;
using System.Linq;

namespace SOSGame.Tests
{
    public class GridSystemUnitTests
    {
        [Fact]
        public void Grid_Initializes_With_Default_Size()
        {
            var gridSystem = new GridSystem();
            Assert.Equal(3, gridSystem.GetGridSize());
        }

        [Fact]
        public void Grid_Updates_Size_When_SetGridSize_Is_Called_With_Valid_Size()
        {
            var gridSystem = new GridSystem();
            gridSystem.SetGridSize(5);
            Assert.Equal(5, gridSystem.GetGridSize());
        }

        [Theory]
        [InlineData(2)]
        [InlineData(13)]
        [InlineData(-1)]
        [InlineData(0)]
        public void SetGridSize_Should_Throw_Exception_When_Invalid_Size_Is_Given(int invalidSize)
        {
            var gridSystem = new GridSystem();
            var ex = Assert.Throws<ArgumentException>(() => gridSystem.SetGridSize(invalidSize));
            Assert.Equal("ERR: The grid size must be between \n 3 and 12.", ex.Message);
        }

        [Fact]
        public void InitializeGrid_Should_Create_Correct_Number_Of_Buttons()
        {
            var gridSystem = new GridSystem();
            gridSystem.SetGridSize(3);
            int actual = gridSystem.Children.OfType<Button>().Count();
            Assert.Equal(9, actual); // 3×3 grid
        }

        [Fact]
        public void InitializeGrid_Should_Update_Buttons_When_Size_Changes()
        {
            var gridSystem = new GridSystem();
            gridSystem.SetGridSize(4);
            int actual = gridSystem.Children.OfType<Button>().Count();
            Assert.Equal(16, actual); // 4×4 grid
        }

        [Fact]
        public void SetCellContent_Paints_Button()
        {
            var grid = new GridSystem();
            grid.SetGridSize(3);

            // Act: paint letter 'S' at (1,1)
            grid.SetCellContent(1, 1, 'S');

            // Find the button at (1,1)
            var btn = grid.Children
                          .OfType<Button>()
                          .FirstOrDefault(b => Grid.GetRow(b) == 1 &&
                                               Grid.GetColumn(b) == 1);

            Assert.NotNull(btn);
            Assert.Equal("S", btn!.Content?.ToString());
        }
    }
}
