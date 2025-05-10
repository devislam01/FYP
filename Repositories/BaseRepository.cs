namespace DemoFYP.Repositories
{
    public abstract class BaseRepository
    {
        public BaseRepository() { 
        
        }

        protected byte[] GenerateUserGuid()
        {
            Guid newGuid = Guid.NewGuid();

            return newGuid.ToByteArray();
        }
    }
}
