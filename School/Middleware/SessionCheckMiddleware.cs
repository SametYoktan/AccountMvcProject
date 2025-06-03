using Serilog;

namespace School.Middleware
{
    public class SessionCheckMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionCheckMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            //bool isAuthenticated =context.User.Identity.IsAuthenticated;
            //Console.WriteLine($">>>>>KULLANICI AKTİFMİ: {isAuthenticated}");

            //// Sadece giriş yapmış kullanıcılar için kontrol yap
            //if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
            //{
            //    Console.WriteLine("Kullanıcı Giriş Yapmış");          
            //    //if (context.Session.GetString("UserId") == null)
            //    //{
            //    //    Console.WriteLine("Oturum süresi dolmuş.");
            //    //}
            //    //else
            //    //{
            //    //    Console.WriteLine("Oturum süresi dolmamış");
            //    //}
            //}
            //else
            //    Console.WriteLine("Kullanıcı Giriş Yapmamış");


            await _next(context);
        }
    }
}
