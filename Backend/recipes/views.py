from django.shortcuts import render, HttpResponse
import json

# Create your views here.
API_KEY = '4a4ab422b0bf44e9b46061f9f4cfde0a'
RECIPE_API_URL = 'https://api.spoonacular.com/recipes/'
INGREDIENTS_API_URL = 'https://api.spoonacular.com/food/ingredients/'
GROCERY_API_URL = 'https://api.spoonacular.com/food/products/'

# ======================= GET request =======================
def get_data(request):
    return HttpResponse("hello world")

# ======================= PUT request =======================
def update_data(request):
    return HttpResponse("hello world")