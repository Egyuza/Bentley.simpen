using Shared;
using Shared.Bentley;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using BCOM = Bentley.Interop.MicroStationDGN;

namespace Embedded.Penetrations.Shared
{
public class SingleModel : NotifyPropertyChangedBase
{
    public delegate void ChangePenetrInfo(PenetrInfo penInfo);

    public event ChangePenetrInfo OnPeneterInfoChanged;

    public BindingList<DiameterType> CurrentDiameters {get; set;}   
    public BindableAttribute value;

    [Bindable(true, BindingDirection.TwoWay)]
    public string Code
    {
        get { return UserTask.Code.ToString(); }
        set { UserTask.Code = value; }
    }
    [Bindable(true, BindingDirection.OneWay)]
    public bool IsCodeValid => !string.IsNullOrEmpty(UserTask.Code); // TODO Code validation


    [Bindable(true, BindingDirection.OneWay)]
    public string TypeSize => 
        $"T{UserTask.FlangesType}-{UserTask.DiameterType?.Number}-{UserTask.LengthCm}";

    [Bindable(true, BindingDirection.OneWay)]
    public string RefPointString => UserTask.Location.ToStringEx();
    [Bindable(true, BindingDirection.TwoWay)]
    public string LengthCm
    {
        get { return UserTask.LengthCm.ToString(); }
        set { UserTask.LengthCm = int.Parse(value); }
    }

    public PenetrUserTask UserTask {get; private set;}

    public SingleModel()
    {
        CurrentDiameters = new BindingList<DiameterType>();
        UserTask = new PenetrUserTask();
    }

    public void startPrimitive()
    {
        PenetrPrimitiveCmd.StartCommand(this);
    }

    public void startDefaultCommand()
    {
        PenetrPrimitiveCmd.StartDefaultCommand();
    }

    public void setLocation(BCOM.Point3d location)
    {
        UserTask.Location = location;
        OnPropertyChanged(nameof(RefPointString));
    }

    public void setFlangeType(long flangeType)
    {
        UserTask.FlangesType = flangeType;

        CurrentDiameters.Clear();

        foreach (DiameterType diamType in 
            PenetrDataSource.Instance.getDiameters((long)flangeType))
        {
            CurrentDiameters.Add(diamType);
        }
        CurrentDiameters.ResetBindings();
    }
   
    public void setDiameterType(object diameterType)
    {
        if (diameterType == null)
            return;

        UserTask.DiameterType = (DiameterType)diameterType;
        
        OnPropertyChanged(nameof(TypeSize));
    }

    public void setLength(int length)
    {
        UserTask.LengthCm = length;        
        OnPropertyChanged(nameof(TypeSize));
    }
}
}
