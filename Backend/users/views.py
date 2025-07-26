from django.shortcuts import render, HttpResponse

# Create your views here.
def auth(request):
    return HttpResponse("User Auth")

# ======================= POST request =======================
def get_existing_materials(request):
    if request.method == "POST":
        data = json.loads(request.body)

        items_list = data.get('items_list')
        #TODO: connect to the database and store

    return HttpResponse("Existing materials fetched successfully")