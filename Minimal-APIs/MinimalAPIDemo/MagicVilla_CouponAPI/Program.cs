using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI;
using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Validation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

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
    APIResponse response = new();
    _logger.Log(LogLevel.Information, "Getting all coupons");
    response.Result = CouponStore.couponList;
    response.IsSuccess = true;
    response.StatusCode = HttpStatusCode.OK;
    return Results.Ok(response);
}).WithName("GetCoupons").Produces < APIResponse > (200);

//get one coupon
app.MapGet("/api/coupon/{id:int}", (ILogger<Program> _logger, int id) =>
{
    APIResponse response = new();
    response.Result = CouponStore.couponList.FirstOrDefault(t => t.Id == id);
    response.IsSuccess = true;
    response.StatusCode = HttpStatusCode.OK;
    return Results.Ok(response);
}).WithName("GetCoupon").Produces<APIResponse>(200);


//create one coupon
app.MapPost("/api/coupon", async (IMapper _mapper, IValidator < CouponCreateDTO > _validation, [FromBody] CouponCreateDTO coupon_C_DTO ) =>
{
    var validationResult = await _validation.ValidateAsync(coupon_C_DTO);

    APIResponse response = new() { IsSuccess=false, StatusCode=HttpStatusCode.BadRequest};


    if (!validationResult.IsValid) 
    {
        response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
        return Results.BadRequest(response);
    }

    if (CouponStore.couponList.FirstOrDefault(t => t.Name.ToLower() == coupon_C_DTO.Name.ToLower()) != null)
    {
        response.ErrorMessages.Add("Coupon name already exist");
        return Results.BadRequest(response);
    }

    Coupon coupon = _mapper.Map<Coupon>(coupon_C_DTO);

    var lastCoupon =  CouponStore.couponList.OrderByDescending(u => u.Id).FirstOrDefault();
    coupon.Id = (lastCoupon?.Id ?? 0) + 1;

    CouponStore.couponList.Add(coupon);

    CouponDTO couponDTO = _mapper.Map<CouponDTO>(coupon);


    response.Result = couponDTO;
    response.IsSuccess = true;
    response.StatusCode = HttpStatusCode.Created;
    return Results.Ok(response);
    //return Results.CreatedAtRoute("GetCoupon", new{id = coupon.Id}, couponDTO);


}).WithName("CreateCoupons").Accepts<CouponCreateDTO>("application/json").Produces<APIResponse>(201).Produces(400);


//put
app.MapPut("/api/coupon", async (IMapper _mapper, IValidator<CouponUpdateDTO> _validation, [FromBody] CouponUpdateDTO coupon_U_DTO) =>
{
    var validationResult = await _validation.ValidateAsync(coupon_U_DTO);

    APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };


    if (!validationResult.IsValid)
    {
        response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
        return Results.BadRequest(response);
    }

    Coupon couponFromStore = CouponStore.couponList.FirstOrDefault(u => u.Id == coupon_U_DTO.Id);
    couponFromStore.IsActive = coupon_U_DTO.IsActive;
    couponFromStore.Name = coupon_U_DTO.Name;
    couponFromStore.Percent = coupon_U_DTO.Percent;
    couponFromStore.LastUpdated = DateTime.Now;

    response.Result = _mapper.Map<CouponDTO>(couponFromStore);
    response.IsSuccess = true;
    response.StatusCode = HttpStatusCode.OK;
    return Results.Ok(response);
}).WithName("UpdateCoupons").Accepts<CouponUpdateDTO>("application/json").Produces<APIResponse>(200).Produces(400);



//delete one coupon
app.MapDelete("/api/coupon/{id:int}", (int id) =>
{
    APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };


    Coupon couponFromStore = CouponStore.couponList.FirstOrDefault(u => u.Id == id);
    if(couponFromStore != null)
    {
        CouponStore.couponList.Remove(couponFromStore);
        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.NoContent;
        return Results.Ok(response);
    }
    else
    {
        response.ErrorMessages.Add("Invalid Id");
        return Results.BadRequest(response);
    }
});


app.UseHttpsRedirection();

app.Run();
