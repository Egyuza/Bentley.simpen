using System.Collections.Generic;
using System.ComponentModel;

namespace Shared
{
/// <summary> Абстрактный класс предоставляющий инструменты нотификации 
/// изменений свойств экземпляров классов-наследников </summary>
public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
{
    // удобно переопределять структуру-список имён изменяемых свойств - NP
    // для каждого класса-наследника, для конроля актуальности имён свойств.//
    // ! в классе-наследнике добавить модификатор new
    public class NP
    {
        // например:
        // public const string Value = "Value";
    }

    /// <summary> Список зависимостей возбуждения нотификаций свойств </summary>
    private static Dictionary<string, HashSet<string>> Dependencies
        = new Dictionary<string, HashSet<string>>();

    /// <summary> Список имён, отн. кот. уже были созданы зависимости </summary>
    private static List<string> InitialDependencies = new List<string>();

    /// <summary> Построение зависимостей возбуждения нотификаций свойств </summary>
    /// <param name="prop"> Имя свойства, кот. должно всегда нотифицироваться, 
    /// после нотификации указанных в "subscriptions" </param>
    /// <param name="subscriptions"> список возбудителей нотификации свойства, 
    /// кот. указано в "prop" </param>
    public static void signOnNotify(string prop, params string[] subscriptions)
    {        
        if (!InitialDependencies.Contains(prop) && subscriptions.Length != 0)
        {
            InitialDependencies.Add(prop);
            foreach (string subProp in subscriptions)
            {
                if (!Dependencies.ContainsKey(subProp))
                {
                    Dependencies.Add(subProp, new HashSet<string>());
                }
                Dependencies[subProp].Add(prop);
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;     
    protected virtual void OnPropertyChanged(string propName)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propName));

            if (Dependencies.ContainsKey(propName))
            {
                // безопасное создание экземпляра перечислителя
                foreach (string dependentPropName in
                    new List<string>(Dependencies[propName]))
                {
                    PropertyChanged(this,
                        new PropertyChangedEventArgs(dependentPropName));
                }
            }
        }
    }
}
}
