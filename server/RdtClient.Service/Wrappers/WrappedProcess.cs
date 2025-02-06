namespace RdtClient.Service.Wrappers;

public class WrappedProcess: IWrappedProcess
{
    public System.Diagnostics.Process NewProcess()
    {
        return new System.Diagnostics.Process();
    }
}
