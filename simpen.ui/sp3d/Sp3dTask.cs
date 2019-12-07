namespace simpen.ui.sp3d
{
class Sp3dTask
{
    public P3DHangerPipeSupport pipe { get; private set; }
    public P3DHangerStdComponent component { get; private set; }

    public Sp3dTask(P3DHangerPipeSupport pipe, P3DHangerStdComponent component)
    {
        this.pipe = pipe;
        this.component = component;
    }

    public bool isFlange()
    {
        return component.Description == "PenFlange";
    }
    public bool isPipe()
    {
        return component.Description == "PenPipe";
    }
    public bool isPipeOld()
    {
        return component.Description == "PntrtPlate-d";
    }
}
}
