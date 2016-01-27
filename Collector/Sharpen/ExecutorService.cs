namespace Sharpen
{
	using System;

	public interface ExecutorService : Executor
	{
		bool AwaitTermination (long n, TimeUnit unit);
		void Shutdown ();
		Future<T> Submit<T> (Callable<T> ob);
	}
}
