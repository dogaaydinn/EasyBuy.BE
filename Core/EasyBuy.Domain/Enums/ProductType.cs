using System.ComponentModel.DataAnnotations;

namespace EasyBuy.Domain.Enums;

public enum ProductType
{
    [Display(Name = "Electronics")]
    Electronics,
    [Display(Name = "Clothing")]
    Clothing,
    [Display(Name = "Shoes")]
    Shoes,
    [Display(Name = "Accessories")]
    Accessories,
    [Display(Name = "Home")]
    Home,
    [Display(Name = "Garden")]
    Garden,
    [Display(Name = "Beauty")]
    Beauty,
    [Display(Name = "Health")]
    Health,
    [Display(Name = "Toys")]
    Toys,
    [Display(Name = "Books")]
    Books,
    [Display(Name = "Food")]
    Food,
    [Display(Name = "Beverages")]
    Beverages,
    [Display(Name = "Sports")]
    Sports,
    [Display(Name = "Outdoors")]
    Outdoors,
    [Display(Name = "Automotive")]
    Automotive,
    [Display(Name = "Industrial")]
    Industrial,
    [Display(Name = "Pets")]
    Pets,
    [Display(Name = "Music")]
    Music,
    [Display(Name = "Movies")]
    Movies,
    [Display(Name = "Games")]
    Games,
    [Display(Name = "Software")]
    Software,
    [Display(Name = "Tools")]
    Tools,
    [Display(Name = "Jewelry")]
    Jewelry,
    [Display(Name = "Watches")]
    Watches,
    [Display(Name = "Baby")]
    Baby,
    [Display(Name = "Kids")]
    Kids,
    [Display(Name = "Office")]
    Office,
    [Display(Name = "Art")]
    Art,
    [Display(Name = "Collectibles")]
    Collectibles,
    [Display(Name = "Crafts")]
    Crafts,
    [Display(Name = "Hobbies")]
    Hobbies,
    [Display(Name = "Travel")]
    Travel,
    [Display(Name = "Services")]
    Services,
    [Display(Name = "Other")]
    Other
}