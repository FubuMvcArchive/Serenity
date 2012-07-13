namespace Serenity.Jasmine
{
	public interface ISpecFileListener
	{
		void Changed();
		void Deleted();
		void Added();
		void Recycle();
	}
}