public abstract class CivilianStateBase
{
    protected readonly Civilian civilian;

    protected CivilianStateBase(Civilian civilian)
    {
        this.civilian = civilian;
    }

    public virtual void Enter() { }
    public virtual void Tick() { }
    public virtual void Exit() { }
}