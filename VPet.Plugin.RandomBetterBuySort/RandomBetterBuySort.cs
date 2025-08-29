using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Panuon.WPF.UI;
using VPet_Simulator.Windows;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.RandomBetterBuySort;

public class RandomBetterBuySort: MainPlugin
{
    public MainWindow RMW { get; private set; }

    // 组件获取
    public ListBox? LsbSortRule => WindowsControlHelper.TryGetControl<ListBox>(RMW.winBetterBuy, "LsbSortRule");
    public ListBox? LsbSortAsc => WindowsControlHelper.TryGetControl<ListBox>(RMW.winBetterBuy, "LsbSortAsc");
    public ListBox? LsbCategory => WindowsControlHelper.TryGetControl<ListBox>(RMW.winBetterBuy, "LsbCategory");
    public TextBox? TbTitleSearch => WindowsControlHelper.TryGetControl<TextBox>(RMW.winBetterBuy, "TbTitleSearch");
    public Pagination? pagination => WindowsControlHelper.TryGetPrivateField<Pagination>(RMW.winBetterBuy, "pagination");
    public ItemsControl? IcCommodity => WindowsControlHelper.TryGetControl<ItemsControl>(RMW.winBetterBuy, "IcCommodity");
    
    // 字段获取
    public int _rows => WindowsControlHelper.TryGetPrivateField<int>(RMW.winBetterBuy, "_rows");
    public int _columns => WindowsControlHelper.TryGetPrivateField<int>(RMW.winBetterBuy, "_columns");
    private bool AllowChange => WindowsControlHelper.TryGetPrivateField<bool>(RMW.winBetterBuy, "AllowChange");

    private static Random random = new Random();

    public Dictionary<object, Func<Food, IComparable>> SortRules { get; private set; } = new()
    {
        {"随机", x => random.Next()}
    };

    public RandomBetterBuySort(IMainWindow mainwin) : base(mainwin)
    {
    }

    public override string PluginName { get; } = "RandomBetterBuySort";
    
    public override void GameLoaded()
    {
        base.GameLoaded();
        RMW = (MainWindow)MW;
        RMW.winBetterBuy.Loaded += WinBetterBuyOnLoaded;
    }

    private void WinBetterBuyOnLoaded(object sender, RoutedEventArgs e)
    {
        var methodInfo = RMW.winBetterBuy.GetType().GetMethod(
            "LsbSortRule_SelectionChanged", 
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new Type[] { typeof(object), typeof(SelectionChangedEventArgs) },
            null
        );
        var eventHandlersStoreProperty = typeof(UIElement).GetProperty(
            "EventHandlersStore", BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (eventHandlersStoreProperty != null)
        {
            // RMW.Dispatcher.Invoke(() => MessageBoxX.Show("宝子处理成功1"));
            var store = eventHandlersStoreProperty.GetValue(LsbSortRule);
            if (store != null)
            {
                // RMW.Dispatcher.Invoke(() => MessageBoxX.Show("宝子处理成功2"));
                var getRoutedEventHandlersMethod = store.GetType().GetMethod("GetRoutedEventHandlers", BindingFlags.Public | BindingFlags.Instance);
                if (getRoutedEventHandlersMethod != null)
                {
                    var handlers = (RoutedEventHandlerInfo[])getRoutedEventHandlersMethod.Invoke(store, new object[] { ListBox.SelectionChangedEvent });
                    // RMW.Dispatcher.Invoke(() => MessageBoxX.Show($"宝子找到路由事件处理程序: {handlers}"));
                    foreach (var handler in handlers)
                    {
                        // 去掉 LsbSortRule 的这些handler绑定
                        var selectionChangedEventHandler = (SelectionChangedEventHandler)handler.Handler;
                        LsbSortRule.SelectionChanged -= selectionChangedEventHandler;
                        LsbCategory.SelectionChanged -= selectionChangedEventHandler;
                        LsbSortAsc.SelectionChanged -= selectionChangedEventHandler;
                        // RMW.Dispatcher.Invoke(() => MessageBoxX.Show($"宝子已移除处理程序: {handler.Handler.Method.Name}"));
                    }
                }
            }
        }
        
        LsbSortRule.SelectionChanged += LsbSortRule_SelectionChanged;
        LsbCategory.SelectionChanged += LsbSortRule_SelectionChanged;
        LsbSortAsc.SelectionChanged += LsbSortRule_SelectionChanged;
        
        foreach (var sortRule in SortRules.Keys)
        {
            LsbSortRule.Items.Add(new ListBoxItem()
            {
                Content = sortRule
            });
        }
    }
    
    private void LsbSortRule_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // RMW.Dispatcher.Invoke(() => MessageBoxX.Show($"宝子处理成功！！！{AllowChange}"));
        if (!AllowChange)
            return;
        int order = LsbSortRule.SelectedIndex;
        bool asc = LsbSortAsc.SelectedIndex == 0;
        MW.Set["betterbuy"].SetInt("lastorder", order);
        MW.Set["betterbuy"].SetBool("lastasc", asc);
        // RMW.Dispatcher.Invoke(() => MessageBoxX.Show("宝子，宝子"));
        OrderItemSource((Food.FoodType)LsbCategory.SelectedIndex, order, asc, TbTitleSearch?.Text);
    }
    
    private IOrderedEnumerable<Food> Order(int sortrule, bool sortasc, List<Food> foods)
    {
        IOrderedEnumerable<Food> ordered;
        // RMW.Dispatcher.Invoke(() => MessageBoxX.Show($"宝子进入order {sortrule}"));
        switch (sortrule)
        {
            case 0:
                if (sortasc)
                    ordered = foods.OrderBy(x => x.TranslateName);
                else
                    ordered = foods.OrderByDescending(x => x.TranslateName);
                break;
            case 1:
                if (sortasc)
                    ordered = foods.OrderBy(x => x.Price);
                else
                    ordered = foods.OrderByDescending(x => x.Price);
                break;
            case 2:
                if (sortasc)
                    ordered = foods.OrderBy(x => x.StrengthFood);
                else
                    ordered = foods.OrderByDescending(x => x.StrengthFood);
                break;
            case 3:
                if (sortasc)
                    ordered = foods.OrderBy(x => x.StrengthDrink);
                else
                    ordered = foods.OrderByDescending(x => x.StrengthDrink);
                break;
            case 4:
                if (sortasc)
                    ordered = foods.OrderBy(x => x.Strength);
                else
                    ordered = foods.OrderByDescending(x => x.Strength);
                break;
            case 5:
                if (sortasc)
                    ordered = foods.OrderBy(x => x.Feeling);
                else
                    ordered = foods.OrderByDescending(x => x.Feeling);
                break;
            case 6:
                if (sortasc)
                    ordered = foods.OrderBy(x => x.Health);
                else
                    ordered = foods.OrderByDescending(x => x.Health);
                break;
            case 7:
                if (sortasc)
                    ordered = foods.OrderBy(x => x.Exp);
                else
                    ordered = foods.OrderByDescending(x => x.Exp);
                break;
            case 8:
                if (sortasc)
                    ordered = foods.OrderBy(x => x.Likability);
                else
                    ordered = foods.OrderByDescending(x => x.Likability);
                break;
            default:
                // RMW.Dispatcher.Invoke(() => MessageBoxX.Show("宝子处理成功haha"));
                ordered = CustomOrder(sortrule, sortasc, foods) ?? Order(0, sortasc, foods);
                break;
        }

        return ordered;
    }

    public IOrderedEnumerable<Food>? CustomOrder(int sortrule, bool sortasc, List<Food> foods)
    {
        try
        {
            ListBoxItem sortRuleObject = null;
            Func<Food, IComparable> rule = null;
            RMW.Dispatcher.Invoke(() =>
            {
                // MessageBoxX.Show("宝子custom");
                sortRuleObject = (ListBoxItem)LsbSortRule.Items.GetItemAt(sortrule);
                rule = SortRules[sortRuleObject.Content];
            });
            
            IOrderedEnumerable<Food> ordered;

            if (sortasc)
            {
                ordered = foods.OrderBy(rule);
                // RMW.Dispatcher.Invoke(() => MessageBoxX.Show($"宝子处理完毕 {sortRuleObject}"));
            }
            else
                ordered = foods.OrderByDescending(rule);
            return ordered;
        }
        catch (Exception e)
        {
            Console.WriteLine($"An Error occurred when ordering foods with custom sort rules {sortrule}: {e.Message}");
            // RMW.Dispatcher.Invoke(() => MessageBoxX.Show($"宝子错误An Error occurred when ordering foods with custom sort rules {sortrule}: {e.Message}"));
        }

        return null;
    }
    
    public void OrderItemSource(Food.FoodType type, int sortrule, bool sortasc, string searchtext = null)
        {
            // RMW.Dispatcher.Invoke(() => MessageBoxX.Show("宝子处理成功OIS"));
            Task.Run(() =>
            {
                // RMW.Dispatcher.Invoke(() => MessageBoxX.Show("宝子处理成功TASK"));
                List<Food> foods;
                switch (type)
                {
                    case Food.FoodType.Food:
                        foods = RMW.Foods;
                        break;
                    case Food.FoodType.Star:
                        //List<Food> lf = new List<Food>();
                        //foreach (var sub in mf.Set["betterbuy"].FindAll("star"))
                        //{
                        //    var str = sub.Info;
                        //    var food = mf.Foods.FirstOrDefault(x => x.Name == str);
                        //    if (food != null)
                        //        lf.Add(food);
                        //}
                        //foods = lf;
                        foods = RMW.Foods.FindAll(x => x.Star);
                        break;
                    default:
                        foods = RMW.Foods.FindAll(x => x.Type == type);// || x.Type == Food.FoodType.Limit);
                        break;
                }
                if (!string.IsNullOrEmpty(searchtext))
                {
                    foods = foods.FindAll(x => x.TranslateName.Contains(searchtext));
                }
                // RMW.Dispatcher.Invoke(() => MessageBoxX.Show("宝子处理成功即将order"));
                IOrderedEnumerable<Food> ordered;
                ordered = Order(sortrule, sortasc, foods);
                RMW.Dispatcher.Invoke(() =>
                {
                    var totalCount = ordered.Count();
                    var pageSize = _rows * _columns;
                    pagination.MaxPage = (int)Math.Ceiling(totalCount * 1.0 / pageSize);
                    var currentPage = Math.Max(0, Math.Min(pagination.MaxPage, pagination.CurrentPage) - 1);
                    pagination.CurrentPage = currentPage + 1;
                    IcCommodity.ItemsSource = ordered.Skip(pageSize * currentPage).Take(pageSize);
                });
            });
        }
}