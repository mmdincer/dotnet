using AutoMapper;
using MagicVilla_CouponAPI;
using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(MappingConfig));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//get all coupons
app.MapGet("/api/coupon", (ILogger<Program> _logger)  =>
{
    _logger.Log(LogLevel.Information, "Getting all coupons");
    return Results.Ok(CouponStore.couponList);
}).WithName("GetCoupons").Produces<IEnumerable<Coupon>>(200);

//get one coupon
app.MapGet("/api/coupon/{id:int}", (int id) =>
{
    return Results.Ok(CouponStore.couponList.FirstOrDefault(t => t.Id == id));
}).WithName("GetCoupon").Produces<Coupon>(200);


//create one coupon
app.MapPost("/api/coupon", (IMapper _mapper,[FromBody] CouponCreateDTO coupon_C_DTO ) =>
{
    if (string.IsNullOrEmpty(coupon_C_DTO.Name)) 
    {
        return Results.BadRequest("Invalid Id or coupon name");
    }

    if (CouponStore.couponList.FirstOrDefault(t => t.Name.ToLower() == coupon_C_DTO.Name.ToLower()) != null)
    {
        return Results.BadRequest("Coupon name already exist");
    }

    Coupon coupon = _mapper.Map<Coupon>(coupon_C_DTO);

    var lastCoupon =  CouponStore.couponList.OrderByDescending(u => u.Id).FirstOrDefault();
    coupon.Id = (lastCoupon?.Id ?? 0) + 1;

    CouponStore.couponList.Add(coupon);

    CouponDTO couponDTO = _mapper.Map<CouponDTO>(coupon);

    return Results.CreatedAtRoute("GetCoupon", new{id = coupon.Id}, couponDTO);

}).WithName("CreateCoupons").Accepts<CouponCreateDTO>("application/json").Produces<CouponDTO>(201).Produces(400);


//put
app.MapPut("/api/coupon", () =>
{

});

//delete one coupon
app.MapDelete("/api/coupon/{id:int}", (int id) =>
{

});


app.UseHttpsRedirection();

app.Run();
